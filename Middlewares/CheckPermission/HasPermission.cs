using JwtTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using JwtTest.Middlewares.CheckPermission;

namespace JwtTest.Middlewares.CheckPermission;

public sealed class HasPermission : TypeFilterAttribute
{
    public HasPermission(AppPermission permission) : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = [permission];
    }
}
