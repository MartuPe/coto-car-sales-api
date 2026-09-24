using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Catalog;
using CarSales.Domain.Sales;

namespace CarSales.UnitTests.Application;

/// <summary>Repositorio de ventas en una lista, para probar los casos de uso sin infraestructura.</summary>
internal sealed class FakeSaleRepository(params Sale[] initialSales) : ISaleRepository
{
    private readonly List<Sale> _sales = [.. initialSales];

    public IReadOnlyList<Sale> Saved => _sales;

    public Task AddAsync(Sale sale, CancellationToken cancellationToken)
    {
        _sales.Add(sale);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Sale>>([.. _sales]);
}

/// <summary>Catálogo con dos modelos: Sedan (sin impuesto extra) y Sport (7 %).</summary>
internal sealed class FakeCarModelRepository : ICarModelRepository
{
    private readonly CarModel[] _models = [TestData.Sedan, TestData.Sport];

    public Task<CarModel?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        Task.FromResult(_models.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<CarModel>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CarModel>>(_models);
}

/// <summary>Dos centros de distribución: Norte (1) y Sur (2).</summary>
internal sealed class FakeDistributionCenterRepository : IDistributionCenterRepository
{
    private readonly DistributionCenter[] _centers = [TestData.North, TestData.South];

    public Task<DistributionCenter?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_centers.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<DistributionCenter>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DistributionCenter>>(_centers);
}

/// <summary>Reloj fijo, para que las fechas de las ventas sean predecibles en los tests.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
