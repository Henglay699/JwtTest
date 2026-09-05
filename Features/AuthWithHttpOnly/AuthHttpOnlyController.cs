using JwtTest.Features.AuthFeature.DTOs;
using JwtTest.Middlewares.CSRF;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtTest.Features.AuthWithHttpOnly;

[Route("api/[controller]")]
[ApiController]
public class AuthHttpOnlyController(IAuthHttpOnly authService, IAntiforgery antiforgery) : ControllerBase
{
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] AuthRequestDto login)
    {
        try
        {
            var result = await authService.LoginAsync(login);
            // if (result is null) return BadRequest("Invalid Credential");

            SetAuthCookies(result);
            return Ok(new { accessTokenExpiresInMinutes = result.ExpiresInMinutes });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var deviceId = Request.Headers["X-Device-Id"].ToString();
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrEmpty(deviceId))
            return Unauthorized("Missing refresh token or device id");

        var result = await authService.RefreshTokenAsync(refreshToken, deviceId);
        if (result is null)
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/" });
            return Unauthorized("Invalid Refresh Token or Device Id");
        }

        SetAuthCookies(result);
        return Ok(new { accessTokenExpiresInMinutes = result.ExpiresInMinutes });
    }

    [HttpPost("Logout")]
    [Authorize]
    [ServiceFilter(typeof(ValidateAntiForgeryTokenFilter))]
    public async Task<IActionResult> Logout()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            await authService.RevokeRefreshTokenAsync(refreshToken!, Request.Headers["X-Device-Id"].ToString());
        }

        ClearAuthCookies();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { csrf_token = tokens.RequestToken });
    }

    private void SetAuthCookies(AuthResponeDto result)
    {
        Response.Cookies.Append("access_token", result.AccesToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(result.ExpiresInMinutes),
            MaxAge = TimeSpan.FromMinutes(result.ExpiresInMinutes),
            Path = "/"
        });

        Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            MaxAge = TimeSpan.FromDays(7),
            Path = "/"
        });
    }

    private void ClearAuthCookies()
    {
        var cookieOptions = new CookieOptions { Path = "/", Secure = true, SameSite = SameSiteMode.None };
        Response.Cookies.Delete("access_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);
    }
}