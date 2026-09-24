using CarSales.Application.Abstractions.Persistence;
using CarSales.Domain.Sales;

namespace CarSales.Infrastructure.Persistence;

/// <summary>
/// Ventas en memoria. Se registra como singleton (una única lista mientras la API está levantada)
/// y ASP.NET Core atiende requests en paralelo, así que el acceso a la lista va dentro de un lock.
/// </summary>
public sealed class InMemorySaleRepository(IEnumerable<Sale> initialSales) : ISaleRepository
{
    private readonly Lock _gate = new();
    private readonly List<Sale> _sales = [.. initialSales];

    public Task AddAsync(Sale sale, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sale);

        lock (_gate)
        {
            _sales.Add(sale);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            // Devuelve una copia: quien la recorre no se ve afectado por ventas que entren mientras tanto.
            return Task.FromResult<IReadOnlyList<Sale>>([.. _sales]);
        }
    }
}
