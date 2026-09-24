using CarSales.Domain.Catalog;

namespace CarSales.Application.Abstractions.Persistence;

/// <summary>Catálogo de modelos de auto (solo lectura: la consigna no pide ABM de entidades).</summary>
public interface ICarModelRepository
{
    /// <summary>Busca un modelo por nombre, sin distinguir mayúsculas de minúsculas.</summary>
    Task<CarModel?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<CarModel>> GetAllAsync(CancellationToken cancellationToken);
}
