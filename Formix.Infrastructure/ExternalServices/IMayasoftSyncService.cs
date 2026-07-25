namespace Formix.Infrastructure.ExternalServices;

// Contrato del servicio que orquesta la sincronización completa: llamar a la API, mapear y guardar en BD
public interface IMayasoftSyncService
{
    Task<int> SincronizarAsync(CancellationToken ct = default);
}
