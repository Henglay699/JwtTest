using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using JwtTest.Models;
using JwtTest.Models.Entities;
using JwtTest.Data;

namespace JwtTest.Middlewares.CheckPermission;

public class PermissionAuthorizationFilter(AppPermission requiredPermission, JwtTestContext dbContext) : IAsyncActionFilter
{
    private readonly AppPermission _requiredPermission = requiredPermission;
    private readonly JwtTestContext _dbContext = dbContext;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated is not true)
        {
            context.Result = new ObjectResult(new { message = "User is not authenticated." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        // Extract User ID from Claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            context.Result = new ObjectResult(new { message = "Missing or invalid user ID." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        // Query database to check if user has the required permission via their roles
        bool hasPermission = await _dbContext.Set<User>()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.Permissions)
            .AnyAsync(p => p.PermissionName == _requiredPermission);


        if (!hasPermission)
        {
            context.Result = new ObjectResult(new { message = "Forbidden: Missing required permission." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        // Permission granted, proceed to controller action
        await next();
    }
}