namespace CarSales.Application.Reports.GetModelShareByCenter;

/// <summary>
/// Participación de cada modelo en cada centro. Los porcentajes se calculan sobre
/// <see cref="TotalUnits"/> (todas las unidades vendidas por la empresa), así que la suma de
/// todas las celdas da 100 % (puede diferir en centésimos por el redondeo a 2 decimales).
/// </summary>
public sealed record ModelShareReportDto(int TotalUnits, IReadOnlyList<CenterModelShareDto> Centers);

/// <summary>Participación de los modelos vendidos en un centro.</summary>
public sealed record CenterModelShareDto(
    int DistributionCenterId,
    string DistributionCenterName,
    IReadOnlyList<ModelShareDto> Models);

/// <summary>Unidades de un modelo vendidas en un centro y su porcentaje sobre el total de la empresa.</summary>
public sealed record ModelShareDto(string Model, int Units, decimal Percentage);
