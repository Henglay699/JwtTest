namespace JwtTest.Features.Auth.DTOs;

public class RefreshTokenDto
{
    public int UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}
