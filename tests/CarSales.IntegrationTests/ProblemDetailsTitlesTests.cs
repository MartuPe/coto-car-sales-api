using CarSales.Api.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarSales.IntegrationTests;

/// <summary>Casos de ProblemDetailsTitles que un request HTTP real de esta API no llega a producir.</summary>
public class ProblemDetailsTitlesTests
{
    [Theory]
    [InlineData(StatusCodes.Status409Conflict)]
    [InlineData(null)]
    public void Apply_UnlistedOrMissingStatus_KeepsTheOriginalTitle(int? status)
    {
        var context = new ProblemDetailsContext
        {
            HttpContext = new DefaultHttpContext(),
            ProblemDetails = new ProblemDetails { Status = status, Title = "Original" },
        };

        ProblemDetailsTitles.Apply(context);

        Assert.Equal("Original", context.ProblemDetails.Title);
    }
}
