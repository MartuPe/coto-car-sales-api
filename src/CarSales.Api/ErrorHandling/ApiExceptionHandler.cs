using CarSales.Application.Common;
using CarSales.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace CarSales.Api.ErrorHandling;

/// <summary>
/// Traduce las excepciones conocidas a respuestas HTTP con formato ProblemDetails (RFC 9457), en un
/// único lugar: los controllers y los casos de uso no tienen try/catch.
/// </summary>
internal sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Datos inválidos"),
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            _ => (0, string.Empty),
        };

        // Cualquier otro error sigue al manejador por defecto: 500 sin exponer detalles internos.
        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = statusCode, Title = title, Detail = exception.Message },
        });
    }
}
