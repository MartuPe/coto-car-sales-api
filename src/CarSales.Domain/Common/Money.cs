namespace CarSales.Domain.Common;

/// <summary>Regla de redondeo para los montos en dólares.</summary>
public static class Money
{
    /// <summary>
    /// Redondea a 2 decimales con redondeo comercial (0,125 → 0,13).
    /// Math.Round usa por defecto el redondeo bancario (0,125 → 0,12),
    /// que no corresponde para montos facturados.
    /// </summary>
    public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
