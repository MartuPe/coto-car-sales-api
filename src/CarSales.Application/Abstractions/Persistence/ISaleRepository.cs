using CarSales.Domain.Sales;

namespace CarSales.Application.Abstractions.Persistence;

/// <summary>
/// Patrón Repository: los casos de uso guardan y leen ventas a través de esta interfaz, sin saber
/// si del otro lado hay una lista en memoria (esta solución) o una base de datos.
/// </summary>
public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken);

    Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken);
}
