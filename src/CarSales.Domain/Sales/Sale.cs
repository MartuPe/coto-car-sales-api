using CarSales.Domain.Catalog;
using CarSales.Domain.Common;

namespace CarSales.Domain.Sales;

/// <summary>
/// Venta de una o más unidades de un modelo en un centro de distribución.
/// Guarda el precio y el impuesto vigentes al momento de la venta: si el precio de lista
/// cambia después, las ventas ya registradas (y los reportes) no se alteran.
/// </summary>
public sealed class Sale
{
    private Sale(
        Guid id,
        int distributionCenterId,
        string modelName,
        int quantity,
        decimal unitPrice,
        decimal taxAmount,
        DateTimeOffset soldAt)
    {
        Id = id;
        DistributionCenterId = distributionCenterId;
        ModelName = modelName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxAmount = taxAmount;
        SoldAt = soldAt;
    }

    public Guid Id { get; }

    public int DistributionCenterId { get; }

    public string ModelName { get; }

    public int Quantity { get; }

    /// <summary>Precio de lista por unidad al momento de la venta, sin impuestos.</summary>
    public decimal UnitPrice { get; }

    public decimal NetAmount => UnitPrice * Quantity;

    public decimal TaxAmount { get; }

    public decimal TotalAmount => NetAmount + TaxAmount;

    public DateTimeOffset SoldAt { get; }

    /// <summary>
    /// Factory method: es la única forma de crear una venta. Valida la cantidad y calcula el
    /// impuesto con la política del modelo, así no puede existir una venta con montos inconsistentes.
    /// </summary>
    /// <exception cref="DomainException">Si la cantidad no es mayor a cero.</exception>
    public static Sale Create(DistributionCenter center, CarModel model, int quantity, DateTimeOffset soldAt)
    {
        ArgumentNullException.ThrowIfNull(center);
        ArgumentNullException.ThrowIfNull(model);

        if (quantity <= 0)
        {
            throw new DomainException($"La cantidad tiene que ser mayor a cero (se recibió {quantity}).");
        }

        var taxAmount = model.TaxPolicy.CalculateTax(model.BasePrice * quantity);

        // Guid v7: incluye la fecha, así los ids quedan ordenados por creación (mejor para índices de base).
        return new Sale(Guid.CreateVersion7(soldAt), center.Id, model.Name, quantity, model.BasePrice, taxAmount, soldAt);
    }
}
