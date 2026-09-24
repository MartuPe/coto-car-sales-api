using CarSales.Application.Abstractions;

namespace CarSales.Application.Diagnostics;

/// <summary>
/// Patrón Decorator: envuelve a cualquier comando con su misma interfaz y mide cuánto tarda,
/// sin que el comando sepa que lo están midiendo. Se registra una sola vez para todos los
/// comandos en <see cref="DependencyInjection.AddApplication"/>.
/// </summary>
public sealed class TimedCommandHandler<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    ExecutionTimer timer) : ICommandHandler<TCommand, TResult>
{
    public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken) =>
        timer.MeasureAsync(
            $"{inner.GetType().Name}.{nameof(HandleAsync)}",
            () => inner.HandleAsync(command, cancellationToken));
}
