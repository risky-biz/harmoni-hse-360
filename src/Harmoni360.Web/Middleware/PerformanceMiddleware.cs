using System.Diagnostics;

namespace Harmoni360.Web.Middleware;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Add performance header before processing request
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedTime = stopwatch.ElapsedMilliseconds;
            
            if (!context.Response.HasStarted)
            {
                context.Response.Headers["X-Response-Time"] = $"{elapsedTime}ms";
            }
            
            return Task.CompletedTask;
        });

        await _next(context);

        stopwatch.Stop();
        var finalElapsedTime = stopwatch.ElapsedMilliseconds;

        if (finalElapsedTime > 1000) // Log if request takes more than 1 second
        {
            _logger.LogWarning("Long running request: {Method} {Path} took {ElapsedTime}ms",
                context.Request.Method,
                context.Request.Path,
                finalElapsedTime);
        }
    }
}