using JwtTest.Data;
using JwtTest.Features.UserFeature.DTOs;
using JwtTest.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtTest.Features.UserFeature;

public class UserService(JwtTestContext _context) : IUserService
{

    // Get User All
    public async Task<List<UserResponse>> GetUsersAsync()
    {
        var users = await _context.Users.Include(r => r.Roles)
                          .Select(u => u.ToDto()).ToListAsync();
        return users;

    }

    // Get User by Id
    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        var users = await _context.Users.Include(r => r.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);
        return users?.ToDto();
    }

    // Create User
    public Task<UserResponse?> CreateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    // Update User
    public Task<UserResponse?> UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    // Delete User
    public Task DeleteUserAsync(List<Guid> ids)
    {
        throw new NotImplementedException();
    }

}
