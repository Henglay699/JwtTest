using JwtTest.Features.UserFeature.DTOs;
using JwtTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JwtTest.Middlewares.CheckPermission;


namespace JwtTest.Features.UserFeature;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [Authorize(Roles = "Admin,HR")]
    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllUsers()
    {
        var user = await userService.GetUsersAsync();
        return Ok(user);
    }

    [HasPermission(AppPermission.ViewUser)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] int id)
    {
        var user = await userService.GetUserByIdAsync(id);
        return (user == null) ? NotFound() : Ok(user);
    }
}
