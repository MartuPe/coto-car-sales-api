namespace CarSales.Domain.Common;

/// <summary>
/// Error de negocio: los datos recibidos no cumplen una regla del dominio
/// (una cantidad no positiva, un centro o un modelo que no existen).
/// La API lo traduce a una respuesta 400 Bad Request.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
