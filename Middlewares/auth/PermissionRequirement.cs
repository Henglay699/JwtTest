using Microsoft.AspNetCore.Authorization;

namespace JwtTest.Middlewares.auth;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission {get;}

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

}
