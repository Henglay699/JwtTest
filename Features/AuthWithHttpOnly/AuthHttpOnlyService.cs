using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtTest.Data;
using JwtTest.Features.AuthFeature.DTOs;
using JwtTest.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace JwtTest.Features.AuthWithHttpOnly;

public class AuthHttpOnlyService(JwtTestContext _context, IConfiguration _config) : IAuthHttpOnly
{

    public async Task<AuthResponeDto?> LoginAsync(AuthRequestDto login)
    {
        var toLowerCaseEmail = login.Email.Trim().ToLower();

        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == toLowerCaseEmail);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password.Trim(), user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is not active");
        return await CreateTokenReponse(user, login.DeviceId);
    }

    private async Task<AuthResponeDto> CreateTokenReponse(User user, string deviceId)
    {
        int expiresInSeconds = int.Parse(_config["Jwt:ExpirationInMinutes"]!);
        return new AuthResponeDto(
            GenerateToken(user),
            await GenerateAndSaveRefreshToken(user, deviceId),
            expiresInSeconds);
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

    // Looked up by the token itself (a unique random 256-bit value) + deviceId,
    // instead of requiring the client to also send UserId. Nothing sensitive
    // needs to travel outside the HttpOnly cookie this way.
    private async Task<User?> ValidateRefreshToken(string refreshToken, string deviceId)
    {
        var storeToken = await _context.RefreshTokens
                        .Include(rt => rt.User)
                        .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.DeviceId == deviceId);

        if (storeToken is null)
            return null;

        if (storeToken.ExpireDate <= DateTime.UtcNow || storeToken.IsInVoked)
            return null;

        return storeToken.User;
    }

    public async Task<AuthResponeDto?> RefreshTokenAsync(string refreshToken, string deviceId)
    {
        var user = await ValidateRefreshToken(refreshToken, deviceId);
        return (user is null) ? null : await CreateTokenReponse(user, deviceId);
    }

    // AuthHttpOnlyService.cs
    public async Task RevokeRefreshTokenAsync(string refreshToken, string deviceId)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.DeviceId == deviceId);

        if (token != null)
        {
            token.IsInVoked = true;
            await _context.SaveChangesAsync();
        }
    }
}