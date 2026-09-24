namespace CarSales.Application.Abstractions;

/// <summary>
/// Caso de uso que solo lee datos, sin modificarlos (CQRS: lado de lectura).
/// Todas las consultas comparten esta interfaz, así un único decorator puede envolverlas a todas.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
