using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Formix.Domain.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Formix.Api;
using Formix.Api.Authorization;

namespace Formix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SysUsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenant;

        public SysUsuarioController(AppDbContext context, IMapper mapper, ITenantContext tenant)
        {
            _context = context;
            _mapper = mapper;
            _tenant = tenant;
        }

        public class RegisterUsuarioRequest
        {
            public int TenantId { get; set; }
            public string NombreUsuario { get; set; } = string.Empty;
            public string? NombreCompleto { get; set; }
            public string? Email { get; set; }
            public string? Telefono { get; set; }
            public bool? Activo { get; set; }
            public bool? Bloqueado { get; set; }
            public string PlainPassword { get; set; } = string.Empty;
            public List<int>? RolIds { get; set; }
        }

        public class UpdatePasswordRequest
        {
            /// <summary>
            /// Clave actual opcional. Si viene se valida antes de actualizar.
            /// </summary>
            public string? ClaveActual { get; set; }

            /// <summary>
            /// Nueva clave requerida.
            /// </summary>
            public string NuevaClave { get; set; } = string.Empty;
        }

        // GET: api/sysusuario
        [HttpGet]
        [RequirePermission(PermissionCodes.VerUsuarios)]
        public async Task<IActionResult> GetUsuarios([FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null)
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var query = _context.SysUsuarios
                    .Include(u => u.SysUsuarioRoles)
                        .ThenInclude(ur => ur.Rol)
                    .Where(u => u.TenantId == tenantId);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(u => 
                        (u.NombreCompleto != null && u.NombreCompleto.ToLower().Contains(term)) ||
                        (u.NombreUsuario != null && u.NombreUsuario.ToLower().Contains(term)) ||
                        (u.Email != null && u.Email.ToLower().Contains(term))
                    );
                }

                if (page.HasValue && pageSize.HasValue)
                {
                    var total = await query.CountAsync();
                    var items = await query
                        .OrderBy(u => u.NombreUsuario)
                        .Skip((Math.Max(1, page.Value) - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .ToListAsync();

                    var itemsDto = _mapper.Map<List<SysUsuarioResponseDto>>(items);
                    return Ok(new { items = itemsDto, total = total });
                }
                else
                {
                    var lista = await query
                        .OrderBy(u => u.NombreUsuario)
                        .ToListAsync();

                    var listaDto = _mapper.Map<List<SysUsuarioResponseDto>>(lista);
                    return Ok(listaDto);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo usuarios: {ex.Message}");
            }
        }

        // POST: api/sysusuario/{id}/password — propia clave sin permiso extra; otra usuario requiere RESETEAR_CLAVE_USUARIO
        [HttpPost("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.NuevaClave))
                {
                    return BadRequest("La nueva clave es requerida");
                }

                var usuario = await _context.SysUsuarios.FindAsync(id);
                if (usuario == null)
                    return NotFound();

                if (!CanManageUsuario(id, PermissionCodes.ResetearClaveUsuario))
                    return Forbid();

                if (!string.IsNullOrWhiteSpace(req.ClaveActual))
                {
                    var esValida = VerifyPassword(req.ClaveActual, usuario.PasswordHash, usuario.PasswordSalt);
                    if (!esValida)
                    {
                        return BadRequest("La clave actual no es correcta");
                    }
                }

                CreatePasswordHash(req.NuevaClave, out var nuevoHash, out var nuevoSalt);
                usuario.PasswordHash = nuevoHash;
                usuario.PasswordSalt = nuevoSalt;
                usuario.IntentosLogin = 0;
                usuario.Bloqueado = false;
                usuario.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Clave actualizada correctamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }

                return Conflict("Conflicto de concurrencia al actualizar la clave del usuario");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la clave del usuario: {ex.Message}");
            }
        }

        // POST: api/sysusuario/register
    //[AllowAnonymous]
        [HttpPost("register")]
        [RequirePermission(PermissionCodes.CrearUsuarios)]
        public async Task<ActionResult<SysUsuarioResponseDto>> Register([FromBody] RegisterUsuarioRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.NombreUsuario) || string.IsNullOrWhiteSpace(req.PlainPassword))
                    return BadRequest("NombreUsuario y PlainPassword son requeridos");

                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var targetTenantId = req.TenantId > 0 ? req.TenantId : tenantId;
                CreatePasswordHash(req.PlainPassword, out var hash, out var salt);

                var entity = new SysUsuario
                {
                    TenantId = targetTenantId,
                    NombreUsuario = req.NombreUsuario.Trim().ToLower(),
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    NombreCompleto = string.IsNullOrWhiteSpace(req.NombreCompleto) 
                        ? req.NombreUsuario.Trim().ToLower() 
                        : req.NombreCompleto.Trim().ToLower(),
                    Email = req.Email?.Trim().ToLower(),
                    Telefono = req.Telefono?.Trim(),
                    UltimoLogin = null,
                    Activo = req.Activo ?? true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow,
                    Bloqueado = req.Bloqueado ?? false,
                    IntentosLogin = 0
                };

                if (req.RolIds != null && req.RolIds.Any())
                {
                    var validos = await _context.SysRoles
                        .Where(r => req.RolIds.Contains(r.RolId) && (r.TenantId == null || r.TenantId == targetTenantId))
                        .Select(r => r.RolId)
                        .ToListAsync();

                    if (validos.Count != req.RolIds.Distinct().Count())
                        return BadRequest("Uno o más roles no existen o no pertenecen al tenant");

                    foreach (var rolId in req.RolIds.Distinct())
                    {
                        entity.SysUsuarioRoles.Add(new SysUsuarioRole
                        {
                            RolId = rolId,
                            FechaAsignacion = DateTime.UtcNow
                        });
                    }
                }

                _context.SysUsuarios.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<SysUsuarioResponseDto>(entity);
                dto.Roles = await GetRolesForUserAsync(entity.UsuarioId, targetTenantId);
                return CreatedAtAction(nameof(GetUsuario), new { id = dto.UsuarioId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el usuario: {ex.Message}");
            }
        }

        // GET: api/sysusuario/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerUsuarios)]
        public async Task<ActionResult<SysUsuarioResponseDto>> GetUsuario(int id)
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var usuario = await _context.SysUsuarios
                    .FirstOrDefaultAsync(u => u.UsuarioId == id && u.TenantId == tenantId);

                if (usuario == null)
                    return NotFound();

                var usuarioDto = _mapper.Map<SysUsuarioResponseDto>(usuario);
                usuarioDto.Roles = await GetRolesForUserAsync(id, tenantId);
                return Ok(usuarioDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el usuario: {ex.Message}");
            }
        }

        // GET: api/sysusuario/5/roles
        [HttpGet("{id}/roles")]
        [RequirePermission(PermissionCodes.VerUsuarios)]
        public async Task<ActionResult<IEnumerable<SysRoleDto>>> GetUsuarioRoles(int id)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await UsuarioInTenantAsync(id, tenantId))
                return NotFound();

            var roles = await GetRolesForUserAsync(id, tenantId);
            return Ok(roles);
        }

        // PUT: api/sysusuario/5/roles — reemplaza todos los roles del usuario
        [HttpPut("{id}/roles")]
        [RequirePermission(PermissionCodes.AsignarRoles)]
        public async Task<IActionResult> SetUsuarioRoles(int id, [FromBody] AssignRolesRequest request)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await UsuarioInTenantAsync(id, tenantId))
                return NotFound();

            var rolIds = (request?.RolIds ?? new List<int>()).Distinct().ToList();

            if (rolIds.Count > 0)
            {
                var validos = await _context.SysRoles
                    .Where(r => rolIds.Contains(r.RolId) && (r.TenantId == null || r.TenantId == tenantId))
                    .Select(r => r.RolId)
                    .ToListAsync();

                if (validos.Count != rolIds.Count)
                    return BadRequest("Uno o más roles no existen o no pertenecen al tenant");
            }

            var actuales = await _context.SysUsuarioRoles
                .Where(ur => ur.UsuarioId == id)
                .ToListAsync();

            _context.SysUsuarioRoles.RemoveRange(actuales);

            foreach (var rolId in rolIds)
            {
                _context.SysUsuarioRoles.Add(new SysUsuarioRole
                {
                    UsuarioId = id,
                    RolId = rolId,
                    FechaAsignacion = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/sysusuario/5/roles — agrega roles al usuario
        [HttpPost("{id}/roles")]
        [RequirePermission(PermissionCodes.AsignarRoles)]
        public async Task<IActionResult> AddUsuarioRoles(int id, [FromBody] AssignRolesRequest request)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await UsuarioInTenantAsync(id, tenantId))
                return NotFound();

            var rolIds = (request?.RolIds ?? new List<int>()).Distinct().ToList();
            if (rolIds.Count == 0)
                return BadRequest("Debe indicar al menos un RolId");

            var validos = await _context.SysRoles
                .Where(r => rolIds.Contains(r.RolId) && (r.TenantId == null || r.TenantId == tenantId))
                .Select(r => r.RolId)
                .ToListAsync();

            if (validos.Count != rolIds.Count)
                return BadRequest("Uno o más roles no existen o no pertenecen al tenant");

            var yaAsignados = await _context.SysUsuarioRoles
                .Where(ur => ur.UsuarioId == id && rolIds.Contains(ur.RolId))
                .Select(ur => ur.RolId)
                .ToListAsync();

            foreach (var rolId in rolIds.Except(yaAsignados))
            {
                _context.SysUsuarioRoles.Add(new SysUsuarioRole
                {
                    UsuarioId = id,
                    RolId = rolId,
                    FechaAsignacion = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/sysusuario/5/roles/3 — quita un rol del usuario
        [HttpDelete("{id}/roles/{rolId}")]
        [RequirePermission(PermissionCodes.AsignarRoles)]
        public async Task<IActionResult> RemoveUsuarioRole(int id, int rolId)
        {
            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            if (!await UsuarioInTenantAsync(id, tenantId))
                return NotFound();

            var link = await _context.SysUsuarioRoles
                .FirstOrDefaultAsync(ur => ur.UsuarioId == id && ur.RolId == rolId);

            if (link == null)
                return NotFound();

            _context.SysUsuarioRoles.Remove(link);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/sysusuario
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearUsuarios)]
        public async Task<ActionResult<SysUsuarioResponseDto>> PostUsuario([FromBody] SysUsuarioDto usuarioDto)
        {
            try
            {
                if (usuarioDto == null)
                    return BadRequest("El cuerpo de la solicitud es requerido");

                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var targetTenantId = usuarioDto.TenantId > 0 ? usuarioDto.TenantId : tenantId;

                var entity = new SysUsuario
                {
                    TenantId = targetTenantId,
                    NombreUsuario = usuarioDto.NombreUsuario.Trim().ToLower(),
                    PasswordHash = usuarioDto.PasswordHash ?? Array.Empty<byte>(),
                    PasswordSalt = usuarioDto.PasswordSalt ?? Array.Empty<byte>(),
                    NombreCompleto = string.IsNullOrWhiteSpace(usuarioDto.NombreCompleto) 
                        ? usuarioDto.NombreUsuario.Trim().ToLower() 
                        : usuarioDto.NombreCompleto.Trim().ToLower(),
                    Email = usuarioDto.Email?.Trim().ToLower(),
                    Telefono = usuarioDto.Telefono?.Trim(),
                    UltimoLogin = usuarioDto.UltimoLogin,
                    Activo = usuarioDto.Activo ?? true,
                    FechaCreacion = usuarioDto.FechaCreacion ?? DateTime.UtcNow,
                    FechaActualizacion = usuarioDto.FechaActualizacion ?? DateTime.UtcNow,
                    Bloqueado = usuarioDto.Bloqueado ?? false,
                    IntentosLogin = usuarioDto.IntentosLogin ?? 0
                };

                // Si no vienen hash/salt, genera credenciales con la clave por defecto "Clave123"
                if (entity.PasswordHash == null || entity.PasswordHash.Length == 0 || entity.PasswordSalt == null || entity.PasswordSalt.Length == 0)
                {
                    CreatePasswordHash("Clave123", out var hash, out var salt);
                    entity.PasswordHash = hash;
                    entity.PasswordSalt = salt;
                }

                _context.SysUsuarios.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<SysUsuarioResponseDto>(entity);

                return CreatedAtAction(nameof(GetUsuario), new { id = dto.UsuarioId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el usuario: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el usuario: {ex.Message}");
            }
        }

        // PUT: api/sysusuario/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarUsuarios)]
        public async Task<IActionResult> PutUsuario(int id, SysUsuarioDto usuarioDto)
        {
            if (id != usuarioDto.UsuarioId)
                return BadRequest();

            if (!TryGetTenant(out var tenantId, out var tenantError))
                return tenantError!;

            var entity = await _context.SysUsuarios.FindAsync(id);
            if (entity == null || entity.TenantId != tenantId)
                return NotFound();

            entity.NombreUsuario = usuarioDto.NombreUsuario.Trim().ToLower();
            entity.NombreCompleto = string.IsNullOrWhiteSpace(usuarioDto.NombreCompleto)
                ? entity.NombreCompleto
                : usuarioDto.NombreCompleto.Trim().ToLower();
            entity.Email = usuarioDto.Email?.Trim().ToLower();
            entity.Telefono = usuarioDto.Telefono?.Trim();
            entity.Activo = usuarioDto.Activo ?? entity.Activo;
            if (entity.Bloqueado == true && usuarioDto.Bloqueado == false)
            {
                entity.IntentosLogin = 0;
            }
            else
            {
                entity.IntentosLogin = usuarioDto.IntentosLogin ?? entity.IntentosLogin;
            }
            entity.Bloqueado = usuarioDto.Bloqueado ?? entity.Bloqueado;
            entity.FechaActualizacion = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Conflicto de concurrencia al actualizar el usuario");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el usuario: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/sysusuario/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarUsuarios)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                if (!TryGetTenant(out var tenantId, out var tenantError))
                    return tenantError!;

                var usuario = await _context.SysUsuarios.FindAsync(id);
                if (usuario == null || usuario.TenantId != tenantId)
                    return NotFound();

                var roles = await _context.SysUsuarioRoles.Where(ur => ur.UsuarioId == id).ToListAsync();
                _context.SysUsuarioRoles.RemoveRange(roles);
                _context.SysUsuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el usuario: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el usuario: {ex.Message}");
            }
        }

        private bool UsuarioExists(int id) =>
            _context.SysUsuarios.Any(e => e.UsuarioId == id);

        private async Task<bool> UsuarioInTenantAsync(int usuarioId, int tenantId) =>
            await _context.SysUsuarios.AnyAsync(u => u.UsuarioId == usuarioId && u.TenantId == tenantId);

        private async Task<List<SysRoleDto>> GetRolesForUserAsync(int usuarioId, int tenantId)
        {
            var roles = await (
                from ur in _context.SysUsuarioRoles
                join r in _context.SysRoles on ur.RolId equals r.RolId
                where ur.UsuarioId == usuarioId
                      && (r.TenantId == null || r.TenantId == tenantId)
                orderby r.Nombre
                select r
            ).ToListAsync();

            return _mapper.Map<List<SysRoleDto>>(roles);
        }

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

        /// <summary>Propio usuario, administrador o permiso indicado.</summary>
        private bool CanManageUsuario(int usuarioId, string? permissionCode = null)
        {
            if (User.HasClaim("es_admin", "true"))
                return true;

            if (!string.IsNullOrEmpty(permissionCode) &&
                User.Claims.Any(c => c.Type == "permission" &&
                    string.Equals(c.Value, permissionCode, StringComparison.OrdinalIgnoreCase)))
                return true;

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return int.TryParse(sub, out var currentId) && currentId == usuarioId;
        }

        private static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, 100_000, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }

        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var rng = RandomNumberGenerator.Create();
            salt = new byte[16];
            rng.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            hash = pbkdf2.GetBytes(32);
        }
    }
}