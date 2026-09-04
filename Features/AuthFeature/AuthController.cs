using JwtTest.Features.AuthFeature.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace JwtTest.Features.AuthFeature
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponeDto>> Login([FromBody] AuthRequestDto login)
        {
            var jwt = await authService.LoginAsync(login);
            return (jwt == null) ? BadRequest("Invalid Credential") : Ok(jwt);
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<AuthResponeDto>> RefreshToken([FromBody] RefreshTokenDto request)
        {
            var result = await authService.RefreshTokenAsync(request);
            return (result is null || result.AccesToken is null
                    || result.RefreshToken is null)
                    ? Unauthorized("Invalid Refresh Token")
                    : Ok(result);
        }
    }
}
