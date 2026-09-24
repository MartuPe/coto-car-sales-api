namespace CarSales.Application.Abstractions;

/// <summary>
/// Caso de uso que modifica el estado (CQRS: lado de escritura).
/// Todos los comandos comparten esta interfaz, así un único decorator puede envolverlos a todos.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
