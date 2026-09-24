using CarSales.Domain.Taxes;

namespace CarSales.Domain.Catalog;

/// <summary>Modelo de auto que fabrica la empresa, con su precio de lista y su política de impuestos.</summary>
public sealed class CarModel
{
    public CarModel(string name, decimal basePrice, ITaxPolicy taxPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(basePrice);
        ArgumentNullException.ThrowIfNull(taxPolicy);

        Name = name;
        BasePrice = basePrice;
        TaxPolicy = taxPolicy;
    }

    public string Name { get; }

    /// <summary>Precio de lista por unidad, en dólares y sin impuestos.</summary>
    public decimal BasePrice { get; }

    public ITaxPolicy TaxPolicy { get; }
}
