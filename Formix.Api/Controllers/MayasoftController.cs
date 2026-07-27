using Formix.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Mvc;

namespace Formix.Api.Controllers;

// Expone el endpoint manual para disparar la sincronización de trámites de MayasoftAPI
[ApiController]
[Route("api/[controller]")]
public class MayasoftController : ControllerBase
{
    private readonly IMayasoftSyncService _syncService;

    public MayasoftController(IMayasoftSyncService syncService)
    {
        _syncService = syncService;
    }

    // POST /api/mayasoft/sincronizar — ejecuta la extracción y guardado bajo demanda
    [HttpPost("sincronizar")]
    public async Task<IActionResult> Sincronizar(CancellationToken ct)
    {
        var procesados = await _syncService.SincronizarAsync(ct);
        return Ok(new { mensaje = "Sincronización completada.", tramitesProcesados = procesados });
    }
}
