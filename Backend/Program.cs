using Microsoft.AspNetCore.Authentication.JwtBearer;

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
        options.Authority = "http://localhost:8080/realms/fullstack-lab";
        options.Audience = "dotnet-api";
        options.RequireHttpsMetadata = false;
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