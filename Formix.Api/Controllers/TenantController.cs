using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Formix.Infrastructure; // Modelo Tenant
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities; // AppDbContext
using Microsoft.AspNetCore.Authorization;
using Formix.Api.Authorization;

namespace FormixBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TenantController(AppDbContext context)
        {
            _context = context;
        }

        // =====================
        // GET: api/Tenant
        // Lista todos los tenants
        // =====================
        [HttpGet]
        [RequirePermission(PermissionCodes.VerTenants)]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants()
        {
            try
            {
                return await _context.Tenants.ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo tenants: {ex.Message}");
            }
        }

        // =====================
        // GET: api/Tenant/{id}
        // Obtiene un tenant por ID
        // =====================
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerTenants)]
        public async Task<ActionResult<Tenant>> GetTenant(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);

                if (tenant == null)
                    return NotFound();

                return tenant;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el tenant: {ex.Message}");
            }
        }

        // =====================
        // GET: api/Tenant/codigoTenant/{codigoNotaria}
        // Obtiene un tenant por CodigoNotaria
        // =====================
        [HttpGet("codigoTenant/{codigoNotaria}")]
        [AllowAnonymous]
        public async Task<ActionResult<Tenant>> GetTenantPorCodigo(string codigoNotaria)
        {
            try
            {
                var tenant = await _context.Tenants.Where(t=>t.CodigoNotaria.Trim().ToUpper() == codigoNotaria.Trim().ToUpper()).FirstOrDefaultAsync();

                if (tenant == null)
                    return NotFound();

                return tenant;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el tenant: {ex.Message}");
            }
        }

        // =====================
        // POST: api/Tenant
        // Crea un nuevo tenant
        // =====================
        [HttpPost]
        [RequirePermission(PermissionCodes.GestionarTenants)]
        public async Task<ActionResult<Tenant>> PostTenant(Tenant tenant)
        {
            try
            {
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTenant), new { id = tenant.TenantId }, tenant);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el tenant: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el tenant: {ex.Message}");
            }
        }

        // =====================
        // PUT: api/Tenant/{id}
        // Actualiza un tenant existente
        // =====================
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.GestionarTenants)]
        public async Task<IActionResult> PutTenant(int id, Tenant tenant)
        {
            if (id != tenant.TenantId)
                return BadRequest();

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(id))
                    return NotFound();
                else
                    return Conflict("Conflicto de concurrencia al actualizar el tenant");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el tenant: {ex.Message}");
            }

            return NoContent();
        }

        // =====================
        // DELETE: api/Tenant/{id}
        // Elimina un tenant
        // =====================
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.GestionarTenants)]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                    return NotFound();

                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el tenant: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el tenant: {ex.Message}");
            }
        }

        // =====================
        // Helper para validar existencia
        // =====================
        private bool TenantExists(int id)
        {
            return _context.Tenants.Any(e => e.TenantId == id);
        }
    }
}
