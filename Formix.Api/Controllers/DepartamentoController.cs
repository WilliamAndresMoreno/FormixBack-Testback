// Controllers/DepartamentoController.cs
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
    public class DepartamentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/departamento
        [HttpGet]
        [RequirePermission(PermissionCodes.VerCatalogos)]
        public async Task<IActionResult> GetDepartamentos()
        {
            try
            {
                var departamentos = await _context.Departamentos
                    .OrderBy(d => d.NombreDepartamento)
                    .ToListAsync();
                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo departamentos: {ex.Message}");
            }
        }
    }
}