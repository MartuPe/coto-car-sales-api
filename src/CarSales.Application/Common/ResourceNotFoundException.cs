namespace CarSales.Application.Common;

/// <summary>El recurso pedido en la URL no existe. La API lo traduce a una respuesta 404 Not Found.</summary>
public sealed class ResourceNotFoundException(string message) : Exception(message);
