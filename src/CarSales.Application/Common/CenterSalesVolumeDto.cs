using CarSales.Domain.Catalog;
using CarSales.Domain.Sales;

namespace CarSales.Application.Common;

/// <summary>Volumen de ventas de un centro de distribución.</summary>
public sealed record CenterSalesVolumeDto(int DistributionCenterId, string DistributionCenterName, SalesVolumeDto Volume)
{
    public static CenterSalesVolumeDto From(DistributionCenter center, SalesVolume volume)
    {
        ArgumentNullException.ThrowIfNull(center);

        return new CenterSalesVolumeDto(center.Id, center.Name, SalesVolumeDto.From(volume));
    }
}
