using Microsoft.AspNetCore.Authorization;

namespace Formix.Api.Authorization;

/// <summary>
/// Exige el permiso indicado (código en Sys_Permisos.Codigo). Los administradores (claim es_admin) pasan siempre.
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = PolicyPrefix + permissionCode;
    }
}
