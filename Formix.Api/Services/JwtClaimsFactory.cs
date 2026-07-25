using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Formix.Api.Services;

public static class JwtClaimsFactory
{
    public const string PermissionClaimType = "permission";
    public const string AdminClaimType = "es_admin";

    public static List<Claim> BuildClaims(int usuarioId, string nombreUsuario, int tenantId, UserAuthorizationInfo auth)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, nombreUsuario),
            new("tenant", tenantId.ToString()),
        };

        if (auth.EsAdministrador)
            claims.Add(new Claim(AdminClaimType, "true"));

        foreach (var role in auth.RoleNames.Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var perm in auth.PermissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(PermissionClaimType, perm));

        return claims;
    }
}
