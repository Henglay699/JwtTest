using JwtTest.Features.UserFeature.DTOs;
using JwtTest.Models.Entities;

namespace JwtTest.Features.UserFeature;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse?> CreateUserAsync(User user);
    Task<UserResponse?> UpdateUserAsync(User user);
    Task DeleteUserAsync(List<Guid> ids);
}
