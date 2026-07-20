using NSwag;
using NSwag.Generation.Processors.Security;

namespace JwtTest.Extensions
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddOpenApiDocument(option =>
            {
                option.Title = "JwtTest";
                option.Version = "v1";
                option.Description = "Jwt Test with API";
                option.DocumentName = "JwtTest";
                option.AddSecurity("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Bearer token authorization header",
                    Type = OpenApiSecuritySchemeType.Http,
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Name = "Authorization",
                    Scheme = "Bearer"
                });
                option.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
            });

            return services;
        }

        public static WebApplication UseSwaggerConfig(this WebApplication webApplication)
        {
            webApplication.UseOpenApi();
            webApplication.UseSwaggerUi();
            return webApplication;
        }
    }
}
 