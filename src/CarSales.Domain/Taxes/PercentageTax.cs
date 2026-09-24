using CarSales.Domain.Common;

namespace CarSales.Domain.Taxes;

/// <summary>Impuesto extra calculado como un porcentaje fijo del monto neto (el 7 % del modelo Sport).</summary>
public sealed class PercentageTax : ITaxPolicy
{
    /// <param name="rate">Alícuota expresada como fracción: 0.07 equivale a 7 %.</param>
    public PercentageTax(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rate);
        Rate = rate;
    }

    public decimal Rate { get; }

    public decimal CalculateTax(decimal netAmount) => Money.Round(netAmount * Rate);
}
