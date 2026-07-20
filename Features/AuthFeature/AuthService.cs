using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtTest.Data;
using JwtTest.Features.Auth.DTOs;
using JwtTest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace JwtTest.Features.AuthFeature;

public class AuthService(JwtTestContext _context, IConfiguration _config) : IAuthService
{

    public async Task<AuthResponeDto?> LoginAsync(AuthRequestDto login)
    {

        var toLowerCaseEmail = login.Email.Trim().ToLower();

        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == toLowerCaseEmail);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password.Trim(), user.PasswordHash))
            return null;
        return await CreateTokenReponse(user, login.DeviceId);
    }

    private async Task<AuthResponeDto> CreateTokenReponse(User user, string deviceId)
    {
        return new AuthResponeDto(GenerateToken(user), await GenerateAndSaveRefreshToken(user, deviceId));
    }


    private string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };
        //foreach (var role in user.Roles.Select(r => r.RoleName).ToList())
        //{
        //    claims.Add(new Claim(ClaimTypes.Role, role));
        //}
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: claims,
                        // expires: DateTime.Now.AddMinutes(15),
                        expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpirationInMinutes"]!)),
                        signingCredentials: creds
                    );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshToken(User user, string deviceId)
    {
        var refreshToken = GenerateRefreshToken();
        var existToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id && rt.DeviceId == deviceId);
        if (existToken != null)
        {
            existToken.Token = refreshToken;
            existToken.IsInVoked = false;
            existToken.ExpireDate = DateTime.UtcNow.AddDays(7);
        }
        else
        {
            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                DeviceId = deviceId,
                IsInVoked = false,
                ExpireDate = DateTime.UtcNow.AddDays(7)
            };
            _context.RefreshTokens.Add(newRefreshToken);
        }

        await _context.SaveChangesAsync();
        return refreshToken;
    }

    private async Task<User?> ValidateRefreshToken(int userId, string refreshToken, string deviceId)
    {
        var storeToken = await _context.RefreshTokens
                        .Include(rt => rt.User)
                        .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storeToken is null || storeToken.Token != refreshToken
            || storeToken.UserId != userId || storeToken.DeviceId != deviceId)
        {
            return null;
        }
        if (storeToken.ExpireDate <= DateTime.UtcNow || storeToken.IsInVoked)
        {
            return null;
        }

        return storeToken.User;
    }

    public async Task<AuthResponeDto?> RefreshTokenAsync(RefreshTokenDto request)
    {
        var user = await ValidateRefreshToken(request.UserId, request.RefreshToken, request.DeviceId);
        return (user is null) ? null : await CreateTokenReponse(user, request.DeviceId);
    }

}
