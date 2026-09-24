using CarSales.Domain.Catalog;
using CarSales.Domain.Sales;
using CarSales.Domain.Taxes;

namespace CarSales.Infrastructure.Persistence;

/// <summary>
/// Datos mockeados que reemplazan a una base de datos, como pide la consigna.
/// Los precios y el impuesto del Sport son los del enunciado.
/// </summary>
public static class MockData
{
    public static IReadOnlyList<CarModel> CarModels { get; } =
    [
        new("Sedan", 8_000m, NoExtraTax.Instance),
        new("SUV", 9_500m, NoExtraTax.Instance),
        new("Offroad", 12_500m, NoExtraTax.Instance),
        new("Sport", 18_200m, new PercentageTax(0.07m)),
    ];

    public static IReadOnlyList<DistributionCenter> DistributionCenters { get; } =
    [
        new(1, "Centro Buenos Aires"),
        new(2, "Centro Córdoba"),
        new(3, "Centro Rosario"),
        new(4, "Centro Mendoza"),
    ];

    /// <summary>
    /// Ventas de septiembre de 2026 con las que arranca la API, para que los reportes tengan datos
    /// desde la primera consulta. Suman 50 unidades, así cada unidad equivale a un 2 % del total.
    /// </summary>
    public static IEnumerable<Sale> CreateInitialSales()
    {
        (int Day, int CenterId, string Model, int Quantity)[] sales =
        [
            (1, 1, "Sedan", 5), (2, 1, "Sedan", 3), (3, 1, "SUV", 4), (4, 1, "Offroad", 2), (5, 1, "Sport", 1),
            (1, 2, "Sedan", 2), (2, 2, "SUV", 3), (3, 2, "SUV", 2), (4, 2, "Offroad", 3), (5, 2, "Sport", 2),
            (1, 3, "Sedan", 4), (2, 3, "SUV", 3), (3, 3, "Offroad", 2), (4, 3, "Sport", 2),
            (1, 4, "Sedan", 2), (2, 4, "SUV", 3), (3, 4, "Offroad", 4), (4, 4, "Offroad", 2), (5, 4, "Sport", 1),
        ];

        return sales.Select(sale => Sale.Create(
            DistributionCenters.Single(c => c.Id == sale.CenterId),
            CarModels.Single(m => m.Name == sale.Model),
            sale.Quantity,
            new DateTimeOffset(2026, 9, sale.Day, 10, 0, 0, TimeSpan.FromHours(-3))));
    }
}
