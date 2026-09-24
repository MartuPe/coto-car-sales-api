using CarSales.Application.Abstractions;
using CarSales.Application.Abstractions.Persistence;
using CarSales.Application.Common;
using CarSales.Domain.Sales;

namespace CarSales.Application.Reports.GetTotalSalesVolume;

/// <summary>Caso de uso «Obtener el volumen de ventas total»: suma todas las ventas de todos los centros.</summary>
public sealed class GetTotalSalesVolumeQueryHandler(ISaleRepository sales)
    : IQueryHandler<GetTotalSalesVolumeQuery, SalesVolumeDto>
{
    public async Task<SalesVolumeDto> HandleAsync(GetTotalSalesVolumeQuery query, CancellationToken cancellationToken)
    {
        var allSales = await sales.GetAllAsync(cancellationToken);

        return SalesVolumeDto.From(SalesVolume.From(allSales));
    }
}
