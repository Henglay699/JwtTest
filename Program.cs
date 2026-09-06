using JwtTest.Extensions;
using JwtTest.Middlewares.CSRF;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

// caching
builder.Services.AddMemoryCache();

// DI Classes
builder.Services.AddApplicationServices();
builder.Services.AddDatabaseServices(builder.Configuration);

// JWT Auth Config
builder.Services.AddJwtAuthServices(builder.Configuration);

// was duplicated twice before - only need to call this once
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = false;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


// THIS was the actual missing piece causing the exception -
// nothing ever registered the filter type itself in DI
builder.Services.AddScoped<ValidateAntiForgeryTokenFilter>();

// still missing from your file - needed for the React app to be able to
// call this API cross-origin with the cookies attached at all
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // set to your actual React dev URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(7127));

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.SeedData();
    app.MapOpenApi();
    app.UseSwaggerConfig();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
}

await app.SeedData();
app.MapOpenApi();
app.UseSwaggerConfig();
app.MapScalarApiReference();

app.UseCors("Frontend"); // must come before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();

app.Run();