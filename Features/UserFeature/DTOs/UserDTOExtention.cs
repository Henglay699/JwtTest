using JwtTest.Models.Entities;

namespace JwtTest.Features.UserFeature.DTOs;

public static class UserDTOExtention
{
    public static UserResponse ToDto(this User request)
    {
        return new UserResponse
        (
            request.Id,
            request.UserName,
            request.Email,
            request.Roles.Select(u => new RoleResponse(
                u.Id, u.RoleName, u.Permissions.Select(p => new PermissionResponse(p.Id, p.PermissionName.ToString())).ToList()
            )).ToList()
        );
    }

    public static User ToEntity(this CreateUserRequest request)
    {
        return new User
        {
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = request.Passsword,
            Roles = new List<Role>()
        };
    }
    public static User ToEntity(this UpdateUserRequest request)
    {
        return new User
        {
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = request.Passsword,
            Roles = new List<Role>()
        };
    }
}
