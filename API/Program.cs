using System.Text;
using CryptoPlatform.API.Services;
using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Application.Interfaces.Services;
using CryptoPlatform.Infrastructure.Data;
using CryptoPlatform.Infrastructure.Data.Repositories;
using CryptoPlatform.Infrastructure.ExternalServices;
using CryptoPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddSingleton<DbConnectionFactory>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Data Service
builder.Services.AddHttpClient<DataServiceClient>();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();

// External Services
builder.Services.AddHttpClient<ICoinGeckoService, CoinGeckoService>(client =>
{
    client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
    client.DefaultRequestHeaders.Add("User-Agent", "CryptoPlatform-ISI-Project");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Swagger with JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CryptoPlatform API",
        Version = "v1",
        Description = "API para gestão de portfolio de criptomoedas"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS (adiciona antes do builder.Build())
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication(); // 👈 IMPORTANTE: antes do UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();