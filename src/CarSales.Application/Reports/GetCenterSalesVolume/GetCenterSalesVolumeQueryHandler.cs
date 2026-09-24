using CarSales.Application.Abstractions;
using CarSales.Application.Abstractions.Persistence;
using CarSales.Application.Common;
using CarSales.Domain.Sales;

namespace CarSales.Application.Reports.GetCenterSalesVolume;

/// <summary>Caso de uso «Obtener el volumen de ventas de un centro» (404 si el centro no existe).</summary>
public sealed class GetCenterSalesVolumeQueryHandler(ISaleRepository sales, IDistributionCenterRepository centers)
    : IQueryHandler<GetCenterSalesVolumeQuery, CenterSalesVolumeDto>
{
    public async Task<CenterSalesVolumeDto> HandleAsync(GetCenterSalesVolumeQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var center = await centers.GetByIdAsync(query.DistributionCenterId, cancellationToken)
            ?? throw new ResourceNotFoundException($"No existe el centro de distribución {query.DistributionCenterId}.");

        var centerSales = (await sales.GetAllAsync(cancellationToken))
            .Where(s => s.DistributionCenterId == center.Id);

        return CenterSalesVolumeDto.From(center, SalesVolume.From(centerSales));
    }
}
