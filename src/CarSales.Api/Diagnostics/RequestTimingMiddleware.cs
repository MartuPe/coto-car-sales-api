using System.Diagnostics;

namespace CarSales.Api.Diagnostics;

/// <summary>
/// Imprime cuánto tarda cada request HTTP completo: routing, validación, caso de uso y serialización.
/// Complementa al decorator de los casos de uso, que mide solo la lógica de negocio: la diferencia entre
/// los dos tiempos muestra cuánto se va en la infraestructura web.
/// </summary>
internal sealed partial class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            LogRequestTime(
                logger,
                context.Request.Method,
                context.Request.Path.ToString(),
                context.Response.StatusCode,
                Stopwatch.GetElapsedTime(start).TotalMilliseconds);
        }
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "HTTP {Method} {Path} respondió {StatusCode} en {ElapsedMilliseconds:0.000} ms")]
    private static partial void LogRequestTime(ILogger logger, string method, string path, int statusCode, double elapsedMilliseconds);
}
