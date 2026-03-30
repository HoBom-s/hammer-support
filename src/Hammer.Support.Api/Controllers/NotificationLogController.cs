using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Support.Api.Controllers;

/// <summary>
///     Read-only endpoints for notification logs (in-app notifications).
/// </summary>
[ApiController]
[Route("api/notification-logs")]
[SuppressMessage("Performance", "CA1515:Consider making public types internal", Justification = "MVC requires public controllers for discovery")]
public sealed class NotificationLogController : ControllerBase
{
    private readonly INotificationLogRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationLogController" /> class.
    /// </summary>
    /// <param name="repository">The notification log repository.</param>
    public NotificationLogController(INotificationLogRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Gets notification logs for a specific recipient.
    /// </summary>
    /// <param name="recipientToken">The recipient token to filter by.</param>
    /// <param name="limit">Maximum number of results (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with the list of notification logs.</returns>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationLog>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByRecipientAsync(
        [FromQuery] string recipientToken,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientToken))
            return BadRequest("recipientToken is required.");

        limit = Math.Clamp(limit, 1, 100);

        IReadOnlyList<NotificationLog> logs = await _repository.GetByRecipientAsync(recipientToken, limit, cancellationToken);
        return Ok(logs);
    }
}
