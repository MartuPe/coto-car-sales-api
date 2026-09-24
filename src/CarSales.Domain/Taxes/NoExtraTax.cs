namespace CarSales.Domain.Taxes;

/// <summary>Política para los modelos que no tienen impuesto extra.</summary>
public sealed class NoExtraTax : ITaxPolicy
{
    /// <summary>Única instancia: no tiene estado, así que no hace falta crear más de una.</summary>
    public static NoExtraTax Instance { get; } = new();

    private NoExtraTax()
    {
    }

    public decimal CalculateTax(decimal netAmount) => 0m;
}
