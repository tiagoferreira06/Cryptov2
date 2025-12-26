using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Infrastructure.Data;
using CryptoPlatform.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddSingleton<DbConnectionFactory>();

// Repositories
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddXmlSerializerFormatters(); // 👈 Suporte para XML

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();

app.Run();