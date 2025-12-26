namespace CryptoPlatform.DataService.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["ApiKey"] ?? "default-insecure-key-change-me";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Permitir acesso ao Swagger sem API Key
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/index.html") ||
            context.Request.Path == "/")
        {
            await _next(context);
            return;
        }

        // Verificar API Key no header
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key não fornecida. Use o header 'X-Api-Key'");
            return;
        }

        if (!_apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("API Key inválida");
            return;
        }

        await _next(context);
    }
}
