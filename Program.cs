using JwtTest.Extensions;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

//caching
builder.Services.AddMemoryCache();


// DI Classses
builder.Services.AddApplicationServices();
builder.Services.AddDatabaseServices(builder.Configuration);

// JWT Auth Config
builder.Services.AddJwtAuthServices(builder.Configuration);

//builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(7127));


var app = builder.Build();

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



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
