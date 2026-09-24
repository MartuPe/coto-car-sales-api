using CarSales.Application.Abstractions;
using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Common;

namespace CarSales.Application.Reports.GetModelShareByCenter;

/// <summary>
/// Caso de uso «Obtener el porcentaje de unidades de cada modelo vendido en cada centro sobre el
/// total de ventas». Arma la matriz completa centro × modelo: las combinaciones sin ventas aparecen
/// con 0 unidades y 0 %, así el consumidor no tiene que deducir los faltantes.
/// </summary>
public sealed class GetModelShareByCenterQueryHandler(
    ISaleRepository sales,
    IDistributionCenterRepository centers,
    ICarModelRepository models) : IQueryHandler<GetModelShareByCenterQuery, ModelShareReportDto>
{
    public async Task<ModelShareReportDto> HandleAsync(GetModelShareByCenterQuery query, CancellationToken cancellationToken)
    {
        var allSales = await sales.GetAllAsync(cancellationToken);
        var allCenters = await centers.GetAllAsync(cancellationToken);
        var allModels = await models.GetAllAsync(cancellationToken);

        var totalUnits = allSales.Sum(s => s.Quantity);

        // Unidades vendidas por cada combinación (centro, modelo), calculadas en una sola pasada.
        var unitsByCenterAndModel = allSales
            .GroupBy(s => (s.DistributionCenterId, s.ModelName))
            .ToDictionary(group => group.Key, group => group.Sum(s => s.Quantity));

        var centerShares = allCenters
            .Select(center => new CenterModelShareDto(
                center.Id,
                center.Name,
                allModels
                    .Select(model =>
                    {
                        var units = unitsByCenterAndModel.GetValueOrDefault((center.Id, model.Name));
                        return new ModelShareDto(model.Name, units, Percentage.Of(units, totalUnits));
                    })
                    .ToList()))
            .ToList();

        return new ModelShareReportDto(totalUnits, centerShares);
    }
}
