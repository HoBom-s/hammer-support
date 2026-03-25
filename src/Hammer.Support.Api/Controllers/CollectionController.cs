using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Support.Api.Controllers;

/// <summary>
///     Manual trigger for data collection jobs. Only available in non-production environments.
/// </summary>
[ApiController]
[Route("api/collections")]
[SuppressMessage("Performance", "CA1515:Consider making public types internal", Justification = "MVC requires public controllers for discovery")]
public sealed class CollectionController : ControllerBase
{
    /// <summary>
    ///     Triggers a KAMCO auction data collection run.
    ///     Returns 409 if a collection is already in progress.
    /// </summary>
    /// <param name="useCase">The collection use case.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with <see cref="CollectionResult"/> on success, or 409 if already running.</returns>
    [HttpPost("kamco")]
    [ProducesResponseType<CollectionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CollectKamcoAsync(
        [FromServices] ICollectKamcoAuctionsUseCase useCase,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(useCase);

        CollectionResult result = await useCase.ExecuteAsync(cancellationToken);

        return result.Skipped
            ? Conflict("Collection already in progress")
            : Ok(result);
    }
}
