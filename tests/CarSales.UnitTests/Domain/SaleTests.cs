using CarSales.Domain.Common;
using CarSales.Domain.Sales;

namespace CarSales.UnitTests.Domain;

public class SaleTests
{
    [Fact]
    public void Create_ModelWithoutExtraTax_ChargesOnlyTheListPrice()
    {
        var sale = Sale.Create(TestData.North, TestData.Sedan, 3, TestData.Now);

        Assert.Equal(8_000m, sale.UnitPrice);
        Assert.Equal(24_000m, sale.NetAmount);
        Assert.Equal(0m, sale.TaxAmount);
        Assert.Equal(24_000m, sale.TotalAmount);
    }

    [Fact]
    public void Create_SportModel_AddsTheSevenPercentTax()
    {
        var sale = Sale.Create(TestData.North, TestData.Sport, 2, TestData.Now);

        Assert.Equal(36_400m, sale.NetAmount);
        Assert.Equal(2_548m, sale.TaxAmount);
        Assert.Equal(38_948m, sale.TotalAmount);
    }

    [Fact]
    public void Create_KeepsCenterModelQuantityAndDate()
    {
        var sale = Sale.Create(TestData.South, TestData.Sport, 1, TestData.Now);

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(TestData.South.Id, sale.DistributionCenterId);
        Assert.Equal("Sport", sale.ModelName);
        Assert.Equal(1, sale.Quantity);
        Assert.Equal(TestData.Now, sale.SoldAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_NonPositiveQuantity_ThrowsDomainException(int quantity)
    {
        var exception = Assert.Throws<DomainException>(
            () => Sale.Create(TestData.North, TestData.Sedan, quantity, TestData.Now));

        Assert.Contains("mayor a cero", exception.Message);
    }

    [Fact]
    public void Create_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Sale.Create(null!, TestData.Sedan, 1, TestData.Now));
        Assert.Throws<ArgumentNullException>(() => Sale.Create(TestData.North, null!, 1, TestData.Now));
    }
}
