namespace CarSales.Domain.Common;

/// <summary>Cálculo de porcentajes para los reportes.</summary>
public static class Percentage
{
    /// <summary>
    /// Devuelve qué porcentaje representa <paramref name="part"/> sobre <paramref name="total"/>,
    /// redondeado a 2 decimales. Si el total es cero devuelve 0 en lugar de dividir por cero.
    /// </summary>
    public static decimal Of(int part, int total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(part);
        ArgumentOutOfRangeException.ThrowIfNegative(total);

        return total == 0 ? 0m : Money.Round(part * 100m / total);
    }
}
