namespace CarSales.Api.ErrorHandling;

/// <summary>
/// Títulos en español de las respuestas de error, definidos en un único lugar. Se aplican a cada
/// ProblemDetails que arma la API: los de <see cref="ApiExceptionHandler"/> y los que genera ASP.NET Core
/// por su cuenta (validación del body, ruta inexistente, método no permitido), que vienen en inglés.
/// </summary>
internal static class ProblemDetailsTitles
{
    private static readonly Dictionary<int, string> _spanishTitles = new()
    {
        [StatusCodes.Status400BadRequest] = "Datos inválidos",
        [StatusCodes.Status404NotFound] = "Recurso no encontrado",
        [StatusCodes.Status405MethodNotAllowed] = "Método no permitido",
        [StatusCodes.Status415UnsupportedMediaType] = "Tipo de contenido no soportado",
        [StatusCodes.Status500InternalServerError] = "Error interno del servidor",
    };

    /// <summary>Reemplaza el título según el código de estado; los códigos no listados quedan como están.</summary>
    public static void Apply(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        if (problem.Status is { } status && _spanishTitles.TryGetValue(status, out var title))
        {
            problem.Title = title;
        }
    }
}
