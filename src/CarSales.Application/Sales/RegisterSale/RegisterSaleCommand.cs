namespace CarSales.Application.Sales.RegisterSale;

/// <summary>Datos para registrar una venta.</summary>
/// <param name="DistributionCenterId">Id del centro de distribución (1 a 4 en los datos mockeados).</param>
/// <param name="Model">Nombre del modelo: Sedan, SUV, Offroad o Sport (sin distinguir mayúsculas).</param>
/// <param name="Quantity">Unidades vendidas, mayor a cero.</param>
public sealed record RegisterSaleCommand(int DistributionCenterId, string Model, int Quantity);
