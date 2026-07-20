using System.ComponentModel.DataAnnotations;

namespace JwtTest.Features.Auth.DTOs;

public record AuthRequestDto
(
    [Required(ErrorMessage = "Username is required.")]
    string Email,
    [Required(ErrorMessage = "Password is required.")]
    string Password,
    [Required(ErrorMessage = "DeviceId is required.")]
    string DeviceId
);
