using Microsoft.AspNetCore.Authorization;

namespace Formix.Api.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string[] permissionCodes)
    {
        PermissionCodes = permissionCodes;
    }

    public string[] PermissionCodes { get; }
}
