using JwtTest.Features.Auth.DTOs;
namespace JwtTest.Features.AuthFeature;

public interface IAuthService
{
    Task<AuthResponeDto?> LoginAsync(AuthRequestDto login);
    Task<AuthResponeDto?> RefreshTokenAsync(RefreshTokenDto request);
}
