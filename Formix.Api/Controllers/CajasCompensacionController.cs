using Formix.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Formix.Api.Authorization;

namespace FormixBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CajasCompensacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CajasCompensacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CajasCompensacion
        [HttpGet]
        public async Task<IActionResult> GetCajasCompensacion()
        {
            try
            {
                var items = await _context.CajasCompensacions
                    .OrderBy(d => d.Nombre)
                    .ToListAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo las cajas de compensacion: {ex.Message}");
            }
        }
    }
}