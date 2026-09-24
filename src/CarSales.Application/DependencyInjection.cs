using CarSales.Application.Abstractions;
using CarSales.Application.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarSales.Application;

public static class DependencyInjection
{
    /// <summary>Registra los casos de uso y los envuelve con el decorator que mide su tiempo de ejecución.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ExecutionTimer>();

        // Scrutor recorre el ensamblado y registra cada clase que implementa ICommandHandler o IQueryHandler.
        // Un caso de uso nuevo queda registrado solo con crear su clase. Se excluyen los decorators genéricos.
        services.Scan(scan => scan
            .FromAssemblyOf<ExecutionTimer>()
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(IQueryHandler<,>))
                .Where(type => !type.IsGenericTypeDefinition))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Decorator: cada caso de uso registrado queda envuelto por el que imprime su tiempo de ejecución.
        services.Decorate(typeof(ICommandHandler<,>), typeof(TimedCommandHandler<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(TimedQueryHandler<,>));

        return services;
    }
}
