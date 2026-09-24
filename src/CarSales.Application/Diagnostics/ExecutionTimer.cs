using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CarSales.Application.Diagnostics;

/// <summary>
/// Mide cuánto tarda una operación y lo imprime en el log, como pide la consigna.
/// Lo usan los decorators de los casos de uso; así ningún caso de uso tiene código de medición.
/// </summary>
public sealed partial class ExecutionTimer(ILogger<ExecutionTimer> logger)
{
    public async Task<T> MeasureAsync<T>(string operation, Func<Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var start = Stopwatch.GetTimestamp();
        try
        {
            return await action();
        }
        finally
        {
            // Se imprime también si la operación falla: el tiempo hasta el error sirve para diagnosticar.
            LogExecutionTime(logger, operation, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
        }
    }

    // Log generado en compilación (LoggerMessage): evita armar el texto si el nivel Information está apagado.
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{Operation} se ejecutó en {ElapsedMilliseconds:0.000} ms")]
    private static partial void LogExecutionTime(ILogger logger, string operation, double elapsedMilliseconds);
}
