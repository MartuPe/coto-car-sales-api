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
        var statusCode = exception switch
        {
            DomainException => StatusCodes.Status400BadRequest,
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            _ => 0,
        };

        // Cualquier otro error sigue al manejador por defecto: 500 sin exponer detalles internos.
        if (statusCode == 0)
        {
            return false;
        }

        // El título lo pone ProblemDetailsTitles según el código, igual que en el resto de los errores.
        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = statusCode, Detail = exception.Message },
        });
    }
}
