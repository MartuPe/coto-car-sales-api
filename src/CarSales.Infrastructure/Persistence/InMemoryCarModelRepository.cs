using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Catalog;

namespace CarSales.Infrastructure.Persistence;

/// <summary>Catálogo de modelos en memoria. Es de solo lectura, así que no necesita sincronización.</summary>
public sealed class InMemoryCarModelRepository : ICarModelRepository
{
    public Task<CarModel?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        Task.FromResult(MockData.CarModels.FirstOrDefault(
            model => string.Equals(model.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<CarModel>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(MockData.CarModels);
}
