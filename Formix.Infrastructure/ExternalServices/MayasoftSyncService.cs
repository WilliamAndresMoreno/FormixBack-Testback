using Formix.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Formix.Infrastructure.ExternalServices;

// Orquesta la sincronización: consulta MayasoftAPI, mapea cada trámite y hace upsert (insertar o actualizar) en la tabla
public class MayasoftSyncService : IMayasoftSyncService
{
    private readonly IMayasoftApiClient _client;
    private readonly AppDbContext _db;
    private readonly ILogger<MayasoftSyncService> _logger;

    public MayasoftSyncService(IMayasoftApiClient client, AppDbContext db, ILogger<MayasoftSyncService> logger)
    {
        _client = client;
        _db = db;
        _logger = logger;
    }

    public async Task<int> SincronizarAsync(CancellationToken ct = default)
    {
        var tramites = await _client.ObtenerTramitesAsync(ct);
        var procesados = 0;

        foreach (var dto in tramites)
        {
            // Busca si el trámite ya existe en la tabla por su Id de Salesforce
            var existente = await _db.TramitesMayasoft
                .FirstOrDefaultAsync(t => t.IdSalesforce == dto.Id, ct);

            if (existente is null)
            {
                _db.TramitesMayasoft.Add(MayasoftTramiteMapper.MapToEntity(dto)); // No existe: se inserta
            }
            else
            {
                MayasoftTramiteMapper.UpdateEntity(existente, dto); // Ya existe: se actualiza
            }
            procesados++;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Sincronización Mayasoft completada: {Count} trámites procesados.", procesados);
        return procesados;
    }
}
