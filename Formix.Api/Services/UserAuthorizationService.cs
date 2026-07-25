using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Formix.Api.Services;

public class UserAuthorizationInfo
{
    public bool EsAdministrador { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public List<string> PermissionCodes { get; set; } = new();
}

public interface IUserAuthorizationService
{
    Task<UserAuthorizationInfo> GetAuthorizationInfoAsync(int usuarioId, int tenantId, CancellationToken ct = default);
    Task<LoginUserInfoDto?> BuildLoginUserInfoAsync(int usuarioId, int tenantId, CancellationToken ct = default);
}

public class UserAuthorizationService : IUserAuthorizationService
{
    private readonly AppDbContext _context;

    public UserAuthorizationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAuthorizationInfo> GetAuthorizationInfoAsync(int usuarioId, int tenantId, CancellationToken ct = default)
    {
        var roleRows = await (
            from ur in _context.SysUsuarioRoles
            join r in _context.SysRoles on ur.RolId equals r.RolId
            where ur.UsuarioId == usuarioId
                  && (r.TenantId == null || r.TenantId == tenantId)
                  && (r.Activo == null || r.Activo == true)
            select new { r.RolId, r.Nombre, r.EsAdministrador }
        ).ToListAsync(ct);

        var esAdmin = roleRows.Any(r => r.EsAdministrador == true);
        var roleIds = roleRows.Select(r => r.RolId).Distinct().ToList();
        var roleNames = roleRows.Select(r => r.Nombre).Distinct().ToList();

        var permissionCodes = new List<string>();
        if (roleIds.Count > 0)
        {
            permissionCodes = await (
                from rp in _context.SysRolPermisos
                join p in _context.SysPermisos on rp.PermisoId equals p.PermisoId
                where roleIds.Contains(rp.RolId)
                select p.Codigo
            ).Distinct().ToListAsync(ct);
        }

        return new UserAuthorizationInfo
        {
            EsAdministrador = esAdmin,
            RoleNames = roleNames,
            PermissionCodes = permissionCodes
        };
    }

    public async Task<LoginUserInfoDto?> BuildLoginUserInfoAsync(int usuarioId, int tenantId, CancellationToken ct = default)
    {
        var user = await _context.SysUsuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId && u.TenantId == tenantId, ct);

        if (user == null) return null;

        var auth = await GetAuthorizationInfoAsync(usuarioId, tenantId, ct);

        return new LoginUserInfoDto
        {
            UsuarioId = user.UsuarioId,
            TenantId = user.TenantId,
            NombreUsuario = user.NombreUsuario,
            NombreCompleto = user.NombreCompleto,
            EsAdministrador = auth.EsAdministrador,
            Roles = auth.RoleNames,
            Permisos = auth.PermissionCodes
        };
    }
}
