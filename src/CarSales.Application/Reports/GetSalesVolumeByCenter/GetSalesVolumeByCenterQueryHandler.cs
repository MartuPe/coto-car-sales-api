using CarSales.Application.Abstractions;
using CarSales.Application.Abstractions.Persistence;
using CarSales.Application.Common;
using CarSales.Domain.Sales;

namespace CarSales.Application.Reports.GetSalesVolumeByCenter;

/// <summary>
/// Caso de uso «Obtener el volumen de ventas por centro». Devuelve todos los centros,
/// también los que todavía no vendieron nada (con volumen en cero).
/// </summary>
public sealed class GetSalesVolumeByCenterQueryHandler(ISaleRepository sales, IDistributionCenterRepository centers)
    : IQueryHandler<GetSalesVolumeByCenterQuery, IReadOnlyList<CenterSalesVolumeDto>>
{
    public async Task<IReadOnlyList<CenterSalesVolumeDto>> HandleAsync(
        GetSalesVolumeByCenterQuery query,
        CancellationToken cancellationToken)
    {
        var salesByCenter = (await sales.GetAllAsync(cancellationToken)).ToLookup(s => s.DistributionCenterId);
        var allCenters = await centers.GetAllAsync(cancellationToken);

        return allCenters
            .Select(center => CenterSalesVolumeDto.From(center, SalesVolume.From(salesByCenter[center.Id])))
            .ToList();
    }
}
