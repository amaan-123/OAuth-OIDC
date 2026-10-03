using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// ==========================================
// 1. SERVICE REGISTRATION (Order generally does not matter)
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.Authority =
        "http://localhost:8080/realms/fullstack-lab";

    options.Audience = "dotnet-api";

    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;

            var realmAccess = context.Principal?
                .FindFirst("realm_access")?.Value;

            if (identity != null && realmAccess != null)
            {
                using var json = JsonDocument.Parse(realmAccess);

                if (json.RootElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        identity.AddClaim(
                            new Claim(ClaimTypes.Role, role.GetString()!)
                        );
                    }
                }
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// ==========================================
// 2. MIDDLEWARE PIPELINE (Order is CRITICAL)
// ==========================================

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();