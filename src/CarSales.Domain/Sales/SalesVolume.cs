namespace CarSales.Domain.Sales;

/// <summary>Totales de un conjunto de ventas: unidades vendidas y montos neto, de impuestos y total.</summary>
public sealed record SalesVolume(int Units, decimal NetAmount, decimal TaxAmount)
{
    public static SalesVolume Empty { get; } = new(0, 0m, 0m);

    public decimal TotalAmount => NetAmount + TaxAmount;

    /// <summary>Suma las unidades y los montos de las ventas recibidas.</summary>
    public static SalesVolume From(IEnumerable<Sale> sales)
    {
        ArgumentNullException.ThrowIfNull(sales);

        var units = 0;
        var netAmount = 0m;
        var taxAmount = 0m;

        foreach (var sale in sales)
        {
            units += sale.Quantity;
            netAmount += sale.NetAmount;
            taxAmount += sale.TaxAmount;
        }

        return new SalesVolume(units, netAmount, taxAmount);
    }
}
