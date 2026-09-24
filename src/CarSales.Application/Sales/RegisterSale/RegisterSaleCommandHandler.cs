using CarSales.Application.Abstractions;
using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Common;
using CarSales.Domain.Sales;

namespace CarSales.Application.Sales.RegisterSale;

/// <summary>
/// Caso de uso «Insertar una venta»: busca el centro y el modelo, crea la venta y la guarda.
/// El cálculo de precios e impuestos vive en el dominio (<see cref="Sale.Create"/>), no acá.
/// </summary>
public sealed class RegisterSaleCommandHandler(
    IDistributionCenterRepository centers,
    ICarModelRepository models,
    ISaleRepository sales,
    TimeProvider timeProvider) : ICommandHandler<RegisterSaleCommand, SaleDto>
{
    public async Task<SaleDto> HandleAsync(RegisterSaleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var center = await centers.GetByIdAsync(command.DistributionCenterId, cancellationToken);
        if (center is null)
        {
            var validIds = (await centers.GetAllAsync(cancellationToken)).Select(c => c.Id);
            throw new DomainException(
                $"No existe el centro de distribución {command.DistributionCenterId}. " +
                $"Centros válidos: {string.Join(", ", validIds)}.");
        }

        var model = await models.GetByNameAsync(command.Model, cancellationToken);
        if (model is null)
        {
            var validNames = (await models.GetAllAsync(cancellationToken)).Select(m => m.Name);
            throw new DomainException(
                $"No existe el modelo '{command.Model}'. Modelos válidos: {string.Join(", ", validNames)}.");
        }

        var sale = Sale.Create(center, model, command.Quantity, timeProvider.GetUtcNow());
        await sales.AddAsync(sale, cancellationToken);

        return SaleDto.From(sale, center);
    }
}
