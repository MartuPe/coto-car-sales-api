namespace CarSales.Domain.Taxes;

/// <summary>
/// Patrón Strategy: cada implementación sabe calcular el impuesto extra de un modelo.
/// El modelo de auto tiene asignada su política y la venta la usa sin saber cuál es.
/// Una regla nueva (por escalas, por precio, por provincia) se agrega con una clase nueva,
/// sin modificar la venta ni los reportes (principio abierto/cerrado).
/// </summary>
public interface ITaxPolicy
{
    /// <summary>Devuelve el impuesto que corresponde a un monto neto, ya redondeado.</summary>
    decimal CalculateTax(decimal netAmount);
}
