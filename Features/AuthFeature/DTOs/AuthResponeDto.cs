namespace JwtTest.Features.AuthFeature.DTOs;

public record AuthResponeDto
(
    string AccesToken,
    string RefreshToken,
    int ExpiresInMinutes
);
