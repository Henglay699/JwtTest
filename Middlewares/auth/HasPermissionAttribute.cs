using JwtTest.Models;
using Microsoft.AspNetCore.Authorization;

namespace JwtTest.Middlewares.auth;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(AppPermission permission)
        : base(policy: permission.ToString())
    {
    }
}
