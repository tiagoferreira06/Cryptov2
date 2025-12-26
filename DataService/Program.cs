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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CryptoPlatform DataService",
        Version = "v1",
        Description = "Serviço de dados XML para gestão de portfolios e transações"
    });
});

var app = builder.Build();

// Habilita Swagger em todos os ambientes (desenvolvimento e produção)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DataService v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz (/)
});

// API Key Middleware para segurança
app.UseMiddleware<CryptoPlatform.DataService.Middleware.ApiKeyMiddleware>();

app.UseCors("AllowAll");
app.MapControllers();

app.Run();