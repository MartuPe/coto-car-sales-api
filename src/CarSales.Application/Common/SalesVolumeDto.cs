using CarSales.Domain.Sales;

namespace CarSales.Application.Common;

/// <summary>
/// Volumen de ventas: unidades vendidas y montos en dólares. El total incluye el impuesto extra
/// del modelo Sport, que además se informa por separado.
/// </summary>
public sealed record SalesVolumeDto(int Units, decimal NetAmount, decimal TaxAmount, decimal TotalAmount)
{
    public static SalesVolumeDto From(SalesVolume volume)
    {
        ArgumentNullException.ThrowIfNull(volume);

        return new SalesVolumeDto(volume.Units, volume.NetAmount, volume.TaxAmount, volume.TotalAmount);
    }
}
