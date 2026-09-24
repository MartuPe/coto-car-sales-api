using CarSales.Api.Contracts;
using CarSales.Application.Abstractions;
using CarSales.Application.Common;
using CarSales.Application.Reports.GetCenterSalesVolume;
using CarSales.Application.Reports.GetModelShareByCenter;
using CarSales.Application.Reports.GetSalesVolumeByCenter;
using CarSales.Application.Reports.GetTotalSalesVolume;
using CarSales.Application.Sales.RegisterSale;
using Microsoft.AspNetCore.Mvc;

namespace CarSales.Api.Controllers;

/// <summary>
/// Servicios REST de ventas. El controller no tiene lógica: traduce HTTP a un caso de uso y devuelve
/// su resultado. Cada acción recibe solo el caso de uso que necesita ([FromServices]).
/// </summary>
[ApiController]
[Route("api/sales")]
[Produces("application/json")]
public sealed class SalesController : ControllerBase
{
    /// <summary>Registra una venta de uno o más autos de un modelo en un centro de distribución.</summary>
    /// <remarks>El total incluye el impuesto extra del 7 % si el modelo es Sport.</remarks>
    /// <response code="201">Venta registrada, con el detalle de sus montos.</response>
    /// <response code="400">Datos inválidos, o un centro o modelo que no existe.</response>
    [HttpPost]
    [ProducesResponseType<SaleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SaleDto>> RegisterSale(
        RegisterSaleRequest request,
        [FromServices] ICommandHandler<RegisterSaleCommand, SaleDto> handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterSaleCommand(request.DistributionCenterId, request.Model, request.Quantity);
        var sale = await handler.HandleAsync(command, cancellationToken);

        // 201 sin cabecera Location: no hay un GET por venta porque la consigna no pide CRUD de entidades.
        return StatusCode(StatusCodes.Status201Created, sale);
    }

    /// <summary>Volumen de ventas total de la empresa: unidades y montos (neto, impuesto y total).</summary>
    [HttpGet("volume")]
    [ProducesResponseType<SalesVolumeDto>(StatusCodes.Status200OK)]
    public Task<SalesVolumeDto> GetTotalVolume(
        [FromServices] IQueryHandler<GetTotalSalesVolumeQuery, SalesVolumeDto> handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(new GetTotalSalesVolumeQuery(), cancellationToken);

    /// <summary>Volumen de ventas de cada centro de distribución (incluye los centros sin ventas).</summary>
    [HttpGet("volume/by-center")]
    [ProducesResponseType<IReadOnlyList<CenterSalesVolumeDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<CenterSalesVolumeDto>> GetVolumeByCenter(
        [FromServices] IQueryHandler<GetSalesVolumeByCenterQuery, IReadOnlyList<CenterSalesVolumeDto>> handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(new GetSalesVolumeByCenterQuery(), cancellationToken);

    /// <summary>Volumen de ventas de un centro de distribución.</summary>
    /// <param name="distributionCenterId">Id del centro (1 a 4).</param>
    /// <param name="handler">Caso de uso (lo inyecta el contenedor de dependencias).</param>
    /// <param name="cancellationToken">Se cancela si el cliente corta la conexión.</param>
    /// <response code="404">El centro no existe.</response>
    [HttpGet("volume/by-center/{distributionCenterId:int}")]
    [ProducesResponseType<CenterSalesVolumeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<CenterSalesVolumeDto> GetCenterVolume(
        int distributionCenterId,
        [FromServices] IQueryHandler<GetCenterSalesVolumeQuery, CenterSalesVolumeDto> handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(new GetCenterSalesVolumeQuery(distributionCenterId), cancellationToken);

    /// <summary>
    /// Porcentaje de unidades de cada modelo vendido en cada centro sobre el total de ventas de la empresa.
    /// </summary>
    /// <remarks>Las celdas centro × modelo suman 100 % (con diferencias de centésimos por el redondeo).</remarks>
    [HttpGet("model-share-by-center")]
    [ProducesResponseType<ModelShareReportDto>(StatusCodes.Status200OK)]
    public Task<ModelShareReportDto> GetModelShareByCenter(
        [FromServices] IQueryHandler<GetModelShareByCenterQuery, ModelShareReportDto> handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(new GetModelShareByCenterQuery(), cancellationToken);
}
