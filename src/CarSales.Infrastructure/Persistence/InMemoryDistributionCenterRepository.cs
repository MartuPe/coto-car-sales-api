using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Catalog;

namespace CarSales.Infrastructure.Persistence;

/// <summary>Centros de distribución en memoria. Es de solo lectura, así que no necesita sincronización.</summary>
public sealed class InMemoryDistributionCenterRepository : IDistributionCenterRepository
{
    public Task<DistributionCenter?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(MockData.DistributionCenters.FirstOrDefault(center => center.Id == id));

    public Task<IReadOnlyList<DistributionCenter>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(MockData.DistributionCenters);
}
