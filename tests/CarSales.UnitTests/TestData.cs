using CarSales.Domain.Catalog;
using CarSales.Domain.Taxes;

namespace CarSales.UnitTests;

/// <summary>Datos de prueba independientes de los datos mockeados de la API.</summary>
internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    public static CarModel Sedan { get; } = new("Sedan", 8_000m, NoExtraTax.Instance);

    public static CarModel Sport { get; } = new("Sport", 18_200m, new PercentageTax(0.07m));

    public static DistributionCenter North { get; } = new(1, "Centro Norte");

    public static DistributionCenter South { get; } = new(2, "Centro Sur");
}
