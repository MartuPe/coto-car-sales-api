using CarSales.Domain.Common;

namespace CarSales.UnitTests.Domain;

public class CommonTests
{
    [Theory]
    [InlineData(1, 3, 33.33)]
    [InlineData(2, 3, 66.67)]
    [InlineData(5, 20, 25)]
    [InlineData(0, 10, 0)]
    public void Percentage_Of_RoundsToTwoDecimals(int part, int total, decimal expected)
    {
        Assert.Equal(expected, Percentage.Of(part, total));
    }

    [Fact]
    public void Percentage_Of_ZeroTotal_ReturnsZeroInsteadOfDividingByZero()
    {
        Assert.Equal(0m, Percentage.Of(0, 0));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(1, -10)]
    public void Percentage_Of_RejectsNegativeValues(int part, int total)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Percentage.Of(part, total));
    }

    [Theory]
    [InlineData(0.125, 0.13)]
    [InlineData(0.135, 0.14)]
    [InlineData(10.004, 10.00)]
    public void Money_Round_UsesCommercialRounding(decimal amount, decimal expected)
    {
        Assert.Equal(expected, Money.Round(amount));
    }
}
