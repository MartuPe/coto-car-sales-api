using CarSales.Domain.Catalog;
using CarSales.Domain.Sales;

namespace CarSales.Application.Sales.RegisterSale;

/// <summary>Venta registrada, con el detalle de cómo se calcularon sus montos.</summary>
public sealed record SaleDto(
    Guid Id,
    int DistributionCenterId,
    string DistributionCenterName,
    string Model,
    int Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTimeOffset SoldAt)
{
    public static SaleDto From(Sale sale, DistributionCenter center)
    {
        ArgumentNullException.ThrowIfNull(sale);
        ArgumentNullException.ThrowIfNull(center);

        return new SaleDto(
            sale.Id,
            center.Id,
            center.Name,
            sale.ModelName,
            sale.Quantity,
            sale.UnitPrice,
            sale.NetAmount,
            sale.TaxAmount,
            sale.TotalAmount,
            sale.SoldAt);
    }
}
