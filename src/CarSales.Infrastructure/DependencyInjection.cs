using CarSales.Application.Abstractions.Persistence;
using CarSales.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CarSales.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los repositorios en memoria. Son singleton porque los datos tienen que sobrevivir entre
    /// requests. Para pasar a una base real se registran acá las nuevas implementaciones, sin tocar los
    /// casos de uso.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICarModelRepository, InMemoryCarModelRepository>();
        services.AddSingleton<IDistributionCenterRepository, InMemoryDistributionCenterRepository>();
        services.AddSingleton<ISaleRepository>(_ => new InMemorySaleRepository(MockData.CreateInitialSales()));

        return services;
    }
}
