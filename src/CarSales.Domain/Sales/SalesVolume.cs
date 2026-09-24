namespace CarSales.Domain.Sales;

/// <summary>Totales de un conjunto de ventas: unidades vendidas y montos neto, de impuestos y total.</summary>
public sealed record SalesVolume(int Units, decimal NetAmount, decimal TaxAmount)
{
    // 0.00m y no 0m: en decimal la escala forma parte del valor, así los montos siempre se informan con 2 decimales.
    public static SalesVolume Empty { get; } = new(0, 0.00m, 0.00m);

    public decimal TotalAmount => NetAmount + TaxAmount;

    /// <summary>Suma las unidades y los montos de las ventas recibidas.</summary>
    public static SalesVolume From(IEnumerable<Sale> sales)
    {
        ArgumentNullException.ThrowIfNull(sales);

        var units = 0;
        var netAmount = 0.00m;
        var taxAmount = 0.00m;

        foreach (var sale in sales)
        {
            units += sale.Quantity;
            netAmount += sale.NetAmount;
            taxAmount += sale.TaxAmount;
        }

        return new SalesVolume(units, netAmount, taxAmount);
    }
}
