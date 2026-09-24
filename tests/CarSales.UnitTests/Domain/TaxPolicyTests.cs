using CarSales.Domain.Taxes;

namespace CarSales.UnitTests.Domain;

public class TaxPolicyTests
{
    [Fact]
    public void NoExtraTax_AlwaysReturnsZero()
    {
        Assert.Equal(0m, NoExtraTax.Instance.CalculateTax(8_000m));
    }

    [Theory]
    [InlineData(18_200, 1_274)]
    [InlineData(36_400, 2_548)]
    public void PercentageTax_CalculatesSevenPercent(decimal netAmount, decimal expectedTax)
    {
        var tax = new PercentageTax(0.07m);

        Assert.Equal(expectedTax, tax.CalculateTax(netAmount));
    }

    [Fact]
    public void PercentageTax_RoundsHalfAwayFromZero()
    {
        // 0,25 x 10 % = 0,025: el redondeo comercial da 0,03 y el bancario daría 0,02.
        var tax = new PercentageTax(0.10m);

        Assert.Equal(0.03m, tax.CalculateTax(0.25m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.07)]
    public void PercentageTax_RejectsNonPositiveRates(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PercentageTax(rate));
    }
}
