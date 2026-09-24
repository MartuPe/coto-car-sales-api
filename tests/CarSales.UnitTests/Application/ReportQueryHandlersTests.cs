using CarSales.Application.Common;
using CarSales.Application.Reports.GetCenterSalesVolume;
using CarSales.Application.Reports.GetModelShareByCenter;
using CarSales.Application.Reports.GetSalesVolumeByCenter;
using CarSales.Application.Reports.GetTotalSalesVolume;
using CarSales.Domain.Sales;

namespace CarSales.UnitTests.Application;

public class ReportQueryHandlersTests
{
    private readonly FakeDistributionCenterRepository _centers = new();
    private readonly FakeCarModelRepository _models = new();

    // 5 unidades en total: Norte vende 3 Sedan y 1 Sport; Sur vende 1 Sedan y ningún Sport.
    private readonly FakeSaleRepository _sales = new(
        Sale.Create(TestData.North, TestData.Sedan, 3, TestData.Now),
        Sale.Create(TestData.North, TestData.Sport, 1, TestData.Now),
        Sale.Create(TestData.South, TestData.Sedan, 1, TestData.Now));

    [Fact]
    public async Task TotalVolume_AddsAllSales()
    {
        var result = await new GetTotalSalesVolumeQueryHandler(_sales)
            .HandleAsync(new GetTotalSalesVolumeQuery(), CancellationToken.None);

        Assert.Equal(new SalesVolumeDto(5, 50_200m, 1_274m, 51_474m), result);
    }

    [Fact]
    public async Task VolumeByCenter_ReturnsEachCenterWithItsOwnSales()
    {
        var result = await new GetSalesVolumeByCenterQueryHandler(_sales, _centers)
            .HandleAsync(new GetSalesVolumeByCenterQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            north => Assert.Equal(new CenterSalesVolumeDto(1, "Centro Norte", new(4, 42_200m, 1_274m, 43_474m)), north),
            south => Assert.Equal(new CenterSalesVolumeDto(2, "Centro Sur", new(1, 8_000m, 0m, 8_000m)), south));
    }

    [Fact]
    public async Task VolumeByCenter_IncludesCentersWithoutSales()
    {
        var result = await new GetSalesVolumeByCenterQueryHandler(new FakeSaleRepository(), _centers)
            .HandleAsync(new GetSalesVolumeByCenterQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, center => Assert.Equal(new SalesVolumeDto(0, 0m, 0m, 0m), center.Volume));
    }

    [Fact]
    public async Task CenterVolume_ExistingCenter_ReturnsOnlyItsSales()
    {
        var result = await new GetCenterSalesVolumeQueryHandler(_sales, _centers)
            .HandleAsync(new GetCenterSalesVolumeQuery(2), CancellationToken.None);

        Assert.Equal(new CenterSalesVolumeDto(2, "Centro Sur", new(1, 8_000m, 0m, 8_000m)), result);
    }

    [Fact]
    public async Task CenterVolume_UnknownCenter_ThrowsNotFound()
    {
        var handler = new GetCenterSalesVolumeQueryHandler(_sales, _centers);

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => handler.HandleAsync(new GetCenterSalesVolumeQuery(9), CancellationToken.None));

        Assert.Equal("No existe el centro de distribución 9.", exception.Message);
    }

    [Fact]
    public async Task CenterVolume_NullQuery_Throws()
    {
        var handler = new GetCenterSalesVolumeQueryHandler(_sales, _centers);

        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ModelShare_CalculatesEachCellOverTheCompanyTotal()
    {
        var result = await new GetModelShareByCenterQueryHandler(_sales, _centers, _models)
            .HandleAsync(new GetModelShareByCenterQuery(), CancellationToken.None);

        Assert.Equal(5, result.TotalUnits);
        Assert.Collection(
            result.Centers,
            north =>
            {
                Assert.Equal("Centro Norte", north.DistributionCenterName);
                Assert.Equal([new("Sedan", 3, 60m), new("Sport", 1, 20m)], north.Models);
            },
            south =>
            {
                Assert.Equal("Centro Sur", south.DistributionCenterName);
                Assert.Equal([new("Sedan", 1, 20m), new("Sport", 0, 0m)], south.Models);
            });
        Assert.Equal(100m, result.Centers.SelectMany(c => c.Models).Sum(m => m.Percentage));
    }

    [Fact]
    public async Task ModelShare_WithoutSales_ReturnsZeroesInsteadOfDividingByZero()
    {
        var result = await new GetModelShareByCenterQueryHandler(new FakeSaleRepository(), _centers, _models)
            .HandleAsync(new GetModelShareByCenterQuery(), CancellationToken.None);

        Assert.Equal(0, result.TotalUnits);
        Assert.All(result.Centers.SelectMany(c => c.Models), share => Assert.Equal(0m, share.Percentage));
    }
}
