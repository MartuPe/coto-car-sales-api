using CarSales.Domain.Catalog;
using CarSales.Domain.Taxes;

namespace CarSales.UnitTests.Domain;

public class CatalogTests
{
    [Fact]
    public void CarModel_KeepsItsData()
    {
        var model = new CarModel("SUV", 9_500m, NoExtraTax.Instance);

        Assert.Equal("SUV", model.Name);
        Assert.Equal(9_500m, model.BasePrice);
        Assert.Same(NoExtraTax.Instance, model.TaxPolicy);
    }

    [Fact]
    public void CarModel_RejectsInvalidData()
    {
        Assert.Throws<ArgumentException>(() => new CarModel(" ", 9_500m, NoExtraTax.Instance));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CarModel("SUV", 0m, NoExtraTax.Instance));
        Assert.Throws<ArgumentNullException>(() => new CarModel("SUV", 9_500m, null!));
    }

    [Fact]
    public void DistributionCenter_KeepsItsData()
    {
        var center = new DistributionCenter(3, "Centro Este");

        Assert.Equal(3, center.Id);
        Assert.Equal("Centro Este", center.Name);
    }

    [Fact]
    public void DistributionCenter_RejectsInvalidData()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DistributionCenter(0, "Centro Este"));
        Assert.Throws<ArgumentException>(() => new DistributionCenter(3, ""));
    }
}
