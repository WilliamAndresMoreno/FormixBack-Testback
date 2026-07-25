// Controllers/MunicipioController.cs
using Formix.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Formix.Api.Authorization;

namespace Formix.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MunicipioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MunicipioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/municipio?codigoDepartamento=05
        [HttpGet]
        [RequirePermission(PermissionCodes.VerCatalogos)]
        public async Task<IActionResult> GetMunicipios([FromQuery] string codigoDepartamento)
        {
            try
            {
                if (string.IsNullOrEmpty(codigoDepartamento))
                    return BadRequest("Debe enviar un código de departamento");

                var municipios = await _context.Municipios
                    .Where(m => m.CodigoDepartamento == codigoDepartamento)
                    .OrderBy(m => m.NombreMunicipio)
                    .ToListAsync();

                return Ok(municipios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo municipios: {ex.Message}");
            }
        }
    }
}