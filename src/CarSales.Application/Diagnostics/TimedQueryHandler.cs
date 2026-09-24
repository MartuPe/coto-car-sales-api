using CarSales.Application.Abstractions;

namespace CarSales.Application.Diagnostics;

/// <summary>
/// Patrón Decorator: envuelve a cualquier consulta con su misma interfaz y mide cuánto tarda,
/// sin que la consulta sepa que la están midiendo. Se registra una sola vez para todas las
/// consultas en <see cref="DependencyInjection.AddApplication"/>.
/// </summary>
public sealed class TimedQueryHandler<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    ExecutionTimer timer) : IQueryHandler<TQuery, TResult>
{
    public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken) =>
        timer.MeasureAsync(
            $"{inner.GetType().Name}.{nameof(HandleAsync)}",
            () => inner.HandleAsync(query, cancellationToken));
}
