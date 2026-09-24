using CarSales.Domain.Catalog;

namespace CarSales.Application.Abstractions.Persistence;

/// <summary>Centros de distribución (solo lectura: la consigna no pide ABM de entidades).</summary>
public interface IDistributionCenterRepository
{
    Task<DistributionCenter?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<DistributionCenter>> GetAllAsync(CancellationToken cancellationToken);
}
