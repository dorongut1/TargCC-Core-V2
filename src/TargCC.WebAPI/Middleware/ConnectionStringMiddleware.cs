namespace TargCC.WebAPI.Middleware;

/// <summary>
/// Middleware to extract connection string from request headers and store in HttpContext
/// </summary>
public class ConnectionStringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConnectionStringMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public ConnectionStringMiddleware(
        RequestDelegate next,
        ILogger<ConnectionStringMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get connection string from header
        var connectionString = context.Request.Headers["X-Connection-String"].FirstOrDefault();

        // Fallback to default connection if no header provided
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Store in HttpContext.Items for use by endpoints
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            context.Items["ConnectionString"] = connectionString;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class ConnectionStringMiddlewareExtensions
{
    public static IApplicationBuilder UseConnectionString(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ConnectionStringMiddleware>();
    }
}
