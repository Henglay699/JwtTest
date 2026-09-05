using System.Security.Claims;
using System.Text;
using JwtTest.Data;
using JwtTest.Features.AuthFeature;
using JwtTest.Features.AuthWithHttpOnly;
using JwtTest.Features.UserFeature;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace JwtTest.Extensions;

public static class DIServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection service)
    {
        service.AddScoped<IAuthService, AuthService>();
        service.AddScoped<IUserService, UserService>();
        service.AddScoped<IAuthHttpOnly, AuthHttpOnlyService>();
        return service;
    }
    public static IServiceCollection AddDatabaseServices(this IServiceCollection service, IConfiguration _config)
    {
        var connStr = _config.GetConnectionString("HostingConnection");
        service.AddDbContext<JwtTestContext>(option =>
                    option.UseSqlServer(connStr));
        return service;
    }
    public static IServiceCollection AddJwtAuthServices(this IServiceCollection services, IConfiguration _config)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                // NEW: this runs first, before validation even starts. Without it,
                // the handler only looks at the "Authorization" header, finds
                // nothing (since the token is in a cookie now), and every request
                // fails auth with no token ever being validated.
                OnMessageReceived = ctx =>
                {
                    if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                    {
                        ctx.Token = token;
                    }
                    return Task.CompletedTask;
                },

                // Unchanged - runs after OnMessageReceived's token is validated
                OnTokenValidated = async ctx =>
                {
                    var userId = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var db = ctx.HttpContext.RequestServices.GetRequiredService<JwtTestContext>();
                    var cache = ctx.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

                    if (!cache.TryGetValue($"roles:{userId}", out List<string>? roles))
                    {
                        roles = await db.Users.Include(u => u.Roles)
                       .Where(u => u.Id == int.Parse(userId!))
                       .SelectMany(u => u.Roles)
                       .Select(r => r.RoleName).ToListAsync();
                        cache.Set($"roles:{userId}", roles, TimeSpan.FromMinutes(5));
                    }

                    var identity = new ClaimsIdentity();
                    foreach (var role in roles!)
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));

                    ctx.Principal!.AddIdentity(identity);
                }
            };
        });
        services.AddAuthorization();
        return services;

    }
}
