namespace CarSales.Domain.Catalog;

/// <summary>Centro de distribución donde se registran las ventas.</summary>
public sealed class DistributionCenter
{
    public DistributionCenter(int id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string Name { get; }
}
