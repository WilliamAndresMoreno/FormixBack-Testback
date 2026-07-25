using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Formix.Infrastructure.ExternalServices;

// Tarea programada: corre la sincronización de Mayasoft automáticamente cada X horas (configurable en appsettings)
public class MayasoftSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly MayasoftApiOptions _options;
    private readonly ILogger<MayasoftSyncBackgroundService> _logger;

    public MayasoftSyncBackgroundService(
        IServiceProvider services,
        IOptions<MayasoftApiOptions> options,
        ILogger<MayasoftSyncBackgroundService> logger)
    {
        _services = services;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(_options.SyncIntervalHours <= 0 ? 24 : _options.SyncIntervalHours);
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                // Se crea un scope nuevo en cada ejecución porque AppDbContext es "Scoped" y este servicio vive durante toda la app
                using var scope = _services.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IMayasoftSyncService>();
                await syncService.SincronizarAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la sincronización automática de Mayasoft.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
