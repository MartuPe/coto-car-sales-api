using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Sales;
using CarSales.Domain.Taxes;
using CarSales.Infrastructure;
using CarSales.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CarSales.UnitTests.Infrastructure;

public class InMemoryRepositoriesTests
{
    [Fact]
    public void MockData_HasTheModelsAndPricesOfTheExam()
    {
        Assert.Equal(
            [("Sedan", 8_000m), ("SUV", 9_500m), ("Offroad", 12_500m), ("Sport", 18_200m)],
            MockData.CarModels.Select(m => (m.Name, m.BasePrice)));

        var sportTax = Assert.IsType<PercentageTax>(MockData.CarModels.Single(m => m.Name == "Sport").TaxPolicy);
        Assert.Equal(0.07m, sportTax.Rate);
        Assert.All(MockData.CarModels.Where(m => m.Name != "Sport"), m => Assert.Same(NoExtraTax.Instance, m.TaxPolicy));
    }

    [Fact]
    public void MockData_HasFourCentersAndFiftyInitialUnits()
    {
        Assert.Equal([1, 2, 3, 4], MockData.DistributionCenters.Select(c => c.Id));
        Assert.Equal(50, MockData.CreateInitialSales().Sum(s => s.Quantity));
    }

    [Fact]
    public void MockData_InitialSalesAreInUtcLikeTheNewOnes()
    {
        Assert.All(MockData.CreateInitialSales(), sale => Assert.Equal(TimeSpan.Zero, sale.SoldAt.Offset));
    }

    [Theory]
    [InlineData("Sport")]
    [InlineData("sport")]
    [InlineData("SPORT")]
    public async Task CarModels_GetByName_IgnoresCase(string name)
    {
        var model = await new InMemoryCarModelRepository().GetByNameAsync(name, CancellationToken.None);

        Assert.Equal("Sport", model?.Name);
    }

    [Fact]
    public async Task CarModels_UnknownName_ReturnsNull()
    {
        var repository = new InMemoryCarModelRepository();

        Assert.Null(await repository.GetByNameAsync("Coupe", CancellationToken.None));
        Assert.Equal(4, (await repository.GetAllAsync(CancellationToken.None)).Count);
    }

    [Fact]
    public async Task DistributionCenters_FindsByIdAndReturnsNullWhenMissing()
    {
        var repository = new InMemoryDistributionCenterRepository();

        Assert.Equal("Centro Rosario", (await repository.GetByIdAsync(3, CancellationToken.None))?.Name);
        Assert.Null(await repository.GetByIdAsync(9, CancellationToken.None));
        Assert.Equal(4, (await repository.GetAllAsync(CancellationToken.None)).Count);
    }

    [Fact]
    public async Task Sales_AddedSaleIsReturnedAfterTheInitialOnes()
    {
        var initial = Sale.Create(TestData.North, TestData.Sedan, 1, TestData.Now);
        var added = Sale.Create(TestData.South, TestData.Sport, 2, TestData.Now);
        var repository = new InMemorySaleRepository([initial]);

        await repository.AddAsync(added, CancellationToken.None);

        Assert.Equal([initial, added], await repository.GetAllAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Sales_GetAllReturnsACopyThatLaterSalesDoNotChange()
    {
        var repository = new InMemorySaleRepository([]);
        var before = await repository.GetAllAsync(CancellationToken.None);

        await repository.AddAsync(Sale.Create(TestData.North, TestData.Sedan, 1, TestData.Now), CancellationToken.None);

        Assert.Empty(before);
    }

    [Fact]
    public async Task Sales_ConcurrentInserts_AreNotLost()
    {
        var repository = new InMemorySaleRepository([]);

        await Parallel.ForAsync(0, 1_000, async (_, cancellationToken) =>
            await repository.AddAsync(Sale.Create(TestData.North, TestData.Sedan, 1, TestData.Now), cancellationToken));

        Assert.Equal(1_000, (await repository.GetAllAsync(CancellationToken.None)).Count);
    }

    [Fact]
    public async Task Sales_AddNull_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => new InMemorySaleRepository([]).AddAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task AddInfrastructure_RegistersSingletonsStartingWithTheMockedSales()
    {
        using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();

        Assert.Same(provider.GetRequiredService<ISaleRepository>(), provider.GetRequiredService<ISaleRepository>());
        Assert.IsType<InMemoryCarModelRepository>(provider.GetRequiredService<ICarModelRepository>());
        Assert.IsType<InMemoryDistributionCenterRepository>(provider.GetRequiredService<IDistributionCenterRepository>());

        var sales = await provider.GetRequiredService<ISaleRepository>().GetAllAsync(CancellationToken.None);
        Assert.Equal(50, sales.Sum(s => s.Quantity));
    }
}
