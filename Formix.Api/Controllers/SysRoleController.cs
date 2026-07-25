using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Formix.Domain.Dtos;
using Microsoft.AspNetCore.Authorization;
using Formix.Api;
using Formix.Api.Authorization;

namespace Formix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SysRoleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenant;

        public SysRoleController(AppDbContext context, IMapper mapper, ITenantContext tenant)
        {
            _context = context;
            _mapper = mapper;
            _tenant = tenant;
        }

        // GET: api/sysrole
        [HttpGet]
        [RequirePermission(PermissionCodes.VerRoles)]
        public async Task<ActionResult<IEnumerable<SysRoleDto>>> GetRoles()
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var lista = await _context.SysRoles
                    .Where(r => r.TenantId == null || r.TenantId == tenantId)
                    .OrderBy(r => r.Nombre)
                    .ToListAsync();

                return Ok(_mapper.Map<List<SysRoleDto>>(lista));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo roles: {ex.Message}");
            }
        }

        // GET: api/sysrole/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerRoles)]
        public async Task<ActionResult<SysRoleDetailDto>> GetRole(int id)
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var role = await _context.SysRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RolId == id && (r.TenantId == null || r.TenantId == tenantId));

                if (role == null)
                    return NotFound();

                var permisos = await (
                    from rp in _context.SysRolPermisos
                    join p in _context.SysPermisos on rp.PermisoId equals p.PermisoId
                    where rp.RolId == id
                    orderby p.Nombre
                    select p
                ).ToListAsync();

                var dto = _mapper.Map<SysRoleDetailDto>(role);
                dto.Permisos = _mapper.Map<List<SysPermisoDto>>(permisos);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el rol: {ex.Message}");
            }
        }

        // GET: api/sysrole/5/permisos
        [HttpGet("{id}/permisos")]
        [RequirePermission(PermissionCodes.VerPermisos)]
        public async Task<ActionResult<IEnumerable<SysPermisoDto>>> GetRolePermisos(int id)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await RoleAccessibleAsync(id, tenantId))
                return NotFound();

            var permisos = await (
                from rp in _context.SysRolPermisos
                join p in _context.SysPermisos on rp.PermisoId equals p.PermisoId
                where rp.RolId == id
                orderby p.Nombre
                select p
            ).ToListAsync();

            return Ok(_mapper.Map<List<SysPermisoDto>>(permisos));
        }

        // PUT: api/sysrole/5/permisos — reemplaza el conjunto completo de permisos del rol
        [HttpPut("{id}/permisos")]
        [RequirePermission(PermissionCodes.EditarRoles)]
        public async Task<IActionResult> SetRolePermisos(int id, [FromBody] AssignPermisosRequest request)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await RoleAccessibleAsync(id, tenantId))
                return NotFound();

            var permisoIds = (request?.PermisoIds ?? new List<int>()).Distinct().ToList();

            if (permisoIds.Count > 0)
            {
                var existentes = await _context.SysPermisos
                    .Where(p => permisoIds.Contains(p.PermisoId))
                    .Select(p => p.PermisoId)
                    .ToListAsync();

                if (existentes.Count != permisoIds.Count)
                    return BadRequest("Uno o más permisos no existen");
            }

            var actuales = await _context.SysRolPermisos
                .Where(rp => rp.RolId == id)
                .ToListAsync();

            _context.SysRolPermisos.RemoveRange(actuales);

            foreach (var permisoId in permisoIds)
            {
                _context.SysRolPermisos.Add(new SysRolPermiso
                {
                    RolId = id,
                    PermisoId = permisoId
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/sysrole/5/permisos — agrega uno o varios permisos al rol
        [HttpPost("{id}/permisos")]
        [RequirePermission(PermissionCodes.EditarRoles)]
        public async Task<IActionResult> AddRolePermisos(int id, [FromBody] AssignPermisosRequest request)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await RoleAccessibleAsync(id, tenantId))
                return NotFound();

            var permisoIds = (request?.PermisoIds ?? new List<int>()).Distinct().ToList();
            if (permisoIds.Count == 0)
                return BadRequest("Debe indicar al menos un PermisoId");

            var existentes = await _context.SysPermisos
                .Where(p => permisoIds.Contains(p.PermisoId))
                .Select(p => p.PermisoId)
                .ToListAsync();

            if (existentes.Count != permisoIds.Count)
                return BadRequest("Uno o más permisos no existen");

            var yaAsignados = await _context.SysRolPermisos
                .Where(rp => rp.RolId == id && permisoIds.Contains(rp.PermisoId))
                .Select(rp => rp.PermisoId)
                .ToListAsync();

            foreach (var permisoId in permisoIds.Except(yaAsignados))
            {
                _context.SysRolPermisos.Add(new SysRolPermiso { RolId = id, PermisoId = permisoId });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/sysrole/5/permisos/12 — quita un permiso del rol
        [HttpDelete("{id}/permisos/{permisoId}")]
        [RequirePermission(PermissionCodes.EditarRoles)]
        public async Task<IActionResult> RemoveRolePermiso(int id, int permisoId)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await RoleAccessibleAsync(id, tenantId))
                return NotFound();

            var link = await _context.SysRolPermisos
                .FirstOrDefaultAsync(rp => rp.RolId == id && rp.PermisoId == permisoId);

            if (link == null)
                return NotFound();

            _context.SysRolPermisos.Remove(link);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/sysrole
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearRoles)]
        public async Task<ActionResult<SysRoleDto>> PostRole(SysRoleDto roleDto)
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var entity = _mapper.Map<SysRole>(roleDto);
                entity.TenantId = roleDto.TenantId ?? tenantId;
                entity.Activo ??= true;
                entity.EsAdministrador ??= false;

                _context.SysRoles.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<SysRoleDto>(entity);
                return CreatedAtAction(nameof(GetRole), new { id = dto.RolId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el rol: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el rol: {ex.Message}");
            }
        }

        // PUT: api/sysrole/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarRoles)]
        public async Task<IActionResult> PutRole(int id, SysRoleDto roleDto)
        {
            if (id != roleDto.RolId)
                return BadRequest();

            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            var entity = await _context.SysRoles.FindAsync(id);
            if (entity == null || (entity.TenantId != null && entity.TenantId != tenantId))
                return NotFound();

            entity.Nombre = roleDto.Nombre;
            entity.Descripcion = roleDto.Descripcion;
            entity.EsAdministrador = roleDto.EsAdministrador;
            entity.Activo = roleDto.Activo;
            if (roleDto.TenantId.HasValue)
                entity.TenantId = roleDto.TenantId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                    return NotFound();
                return Conflict("Conflicto de concurrencia al actualizar el rol");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el rol: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/sysrole/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarRoles)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var role = await _context.SysRoles.FindAsync(id);
                if (role == null || (role.TenantId != null && role.TenantId != tenantId))
                    return NotFound();

                var links = await _context.SysRolPermisos.Where(rp => rp.RolId == id).ToListAsync();
                var userLinks = await _context.SysUsuarioRoles.Where(ur => ur.RolId == id).ToListAsync();
                _context.SysRolPermisos.RemoveRange(links);
                _context.SysUsuarioRoles.RemoveRange(userLinks);
                _context.SysRoles.Remove(role);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el rol: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el rol: {ex.Message}");
            }
        }

        private bool RoleExists(int id) => _context.SysRoles.Any(e => e.RolId == id);

        private async Task<bool> RoleAccessibleAsync(int rolId, int tenantId) =>
            await _context.SysRoles.AnyAsync(r =>
                r.RolId == rolId && (r.TenantId == null || r.TenantId == tenantId));

        private bool TryGetTenant(out int tenantId, out ActionResult? error)
        {
            tenantId = _tenant.TenantId;
            if (tenantId <= 0)
            {
                error = Unauthorized("Tenant no definido (header X-Tenant-Id)");
                return false;
            }
            error = null;
            return true;
        }
    }
}
