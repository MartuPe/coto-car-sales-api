using CarSales.Domain.Sales;

namespace CarSales.UnitTests.Domain;

public class SalesVolumeTests
{
    [Fact]
    public void From_NoSales_ReturnsEmpty()
    {
        Assert.Equal(SalesVolume.Empty, SalesVolume.From([]));
    }

    [Fact]
    public void From_AddsUnitsAndAmounts()
    {
        var sales = new[]
        {
            Sale.Create(TestData.North, TestData.Sedan, 2, TestData.Now),
            Sale.Create(TestData.South, TestData.Sport, 1, TestData.Now),
        };

        var volume = SalesVolume.From(sales);

        Assert.Equal(3, volume.Units);
        Assert.Equal(34_200m, volume.NetAmount);
        Assert.Equal(1_274m, volume.TaxAmount);
        Assert.Equal(35_474m, volume.TotalAmount);
    }

    [Fact]
    public void From_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SalesVolume.From(null!));
    }
}
