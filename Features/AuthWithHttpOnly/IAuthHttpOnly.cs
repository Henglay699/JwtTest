using JwtTest.Features.AuthFeature.DTOs;
namespace JwtTest.Features.AuthWithHttpOnly;

public interface IAuthHttpOnly
{
    Task<AuthResponeDto?> LoginAsync(AuthRequestDto login);
    Task<AuthResponeDto?> RefreshTokenAsync(string refreshToken, string deviceId);
    Task RevokeRefreshTokenAsync(string refreshToken, string deviceId);
}
