using System.ComponentModel.DataAnnotations;

namespace CarSales.Api.Contracts;

/// <summary>Cuerpo del POST /api/sales. Es el contrato público de la API, separado del comando interno.</summary>
public sealed record RegisterSaleRequest
{
    /// <summary>Id del centro de distribución (1 a 4).</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "El centro de distribución tiene que ser un id positivo.")]
    public required int DistributionCenterId { get; init; }

    /// <summary>Modelo vendido: Sedan, SUV, Offroad o Sport (sin distinguir mayúsculas).</summary>
    /// <example>Sport</example>
    [Required(ErrorMessage = "El modelo es obligatorio.")]
    public required string Model { get; init; }

    /// <summary>Unidades vendidas, entre 1 y 1000 (el tope evita cargar por error cantidades absurdas).</summary>
    /// <example>2</example>
    [Range(1, 1_000, ErrorMessage = "La cantidad tiene que estar entre {1} y {2}.")]
    public required int Quantity { get; init; }
}
