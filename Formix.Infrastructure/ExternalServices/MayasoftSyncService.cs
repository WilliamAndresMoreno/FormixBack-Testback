using Formix.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Formix.Infrastructure.ExternalServices;

// Orquesta la sincronización: consulta MayasoftAPI y, por cada trámite,
// (1) guarda/actualiza la fila de control en TramitesMayasoft y
// (2) crea o actualiza el radicado en las tablas normalizadas de Formix.
public class MayasoftSyncService : IMayasoftSyncService
{
    private readonly IMayasoftApiClient _client;
    private readonly AppDbContext _db;
    private readonly MayasoftRadicadoBuilder _builder;
    private readonly ILogger<MayasoftSyncService> _logger;

    public MayasoftSyncService(
        IMayasoftApiClient client,
        AppDbContext db,
        MayasoftRadicadoBuilder builder,
        ILogger<MayasoftSyncService> logger)
    {
        _client = client;
        _db = db;
        _builder = builder;
        _logger = logger;
    }

    public async Task<int> SincronizarAsync(CancellationToken ct = default)
    {
        var tramites = await _client.ObtenerTramitesAsync(ct);
        var procesados = 0;
        var fallidos = 0;

        foreach (var dto in tramites)
        {
            // Cada trámite va en su propia transacción: si uno falla, no afecta a los demás
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var control = await _db.TramitesMayasoft
                    .FirstOrDefaultAsync(t => t.IdSalesforce == dto.Id, ct);

                if (control is null)
                {
                    control = MayasoftTramiteMapper.MapToEntity(dto);
                    _db.TramitesMayasoft.Add(control);
                }
                else
                {
                    MayasoftTramiteMapper.UpdateEntity(control, dto);
                }

                if (control.IdRadicado is null)
                {
                    // Primera vez que se ve este trámite: se crea el radicado completo y se guarda el vínculo
                    control.IdRadicado = await _builder.CrearRadicadoAsync(dto, ct);
                    _logger.LogInformation("Trámite {Id} -> Radicado {IdRadicado} creado.", dto.Id, control.IdRadicado);
                }
                else
                {
                    // Ya existe: solo se refresca la información que puede cambiar (pagos, contacto, fecha OE)
                    await _builder.ActualizarRadicadoAsync(control.IdRadicado.Value, dto, ct);
                    _logger.LogInformation("Trámite {Id} -> Radicado {IdRadicado} actualizado.", dto.Id, control.IdRadicado);
                }

                control.FechaUltimaSincronizacion = DateTime.Now;
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                procesados++;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _db.ChangeTracker.Clear(); // Se descartan las entidades a medio crear para que no contaminen el siguiente trámite
                fallidos++;
                _logger.LogError(ex, "Error procesando el trámite {Id} de Mayasoft.", dto.Id);
            }
        }

        _logger.LogInformation("Sincronización Mayasoft completada: {Ok} trámites procesados, {Fallidos} con error.", procesados, fallidos);
        return procesados;
    }
}
