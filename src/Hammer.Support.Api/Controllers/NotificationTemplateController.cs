using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Support.Api.Controllers;

/// <summary>
///     CRUD endpoints for notification templates.
/// </summary>
[ApiController]
[Route("api/notification-templates")]
[SuppressMessage("Performance", "CA1515:Consider making public types internal", Justification = "MVC requires public controllers for discovery")]
public sealed class NotificationTemplateController : ControllerBase
{
    private readonly INotificationTemplateService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationTemplateController" /> class.
    /// </summary>
    /// <param name="service">The notification template service.</param>
    public NotificationTemplateController(INotificationTemplateService service)
    {
        _service = service;
    }

    /// <summary>
    ///     Gets all notification templates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with the list of all templates.</returns>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationTemplate>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<NotificationTemplate> templates = await _service.GetAllAsync(cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    ///     Gets a notification template by ID.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with the template, or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<NotificationTemplate>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await _service.GetByIdAsync(id, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }

    /// <summary>
    ///     Creates a new notification template.
    /// </summary>
    /// <param name="request">The create request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created template.</returns>
    [HttpPost]
    [ProducesResponseType<NotificationTemplate>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        CreateNotificationTemplateCommand command = new()
        {
            TemplateKey = request.TemplateKey,
            TitleTemplate = request.TitleTemplate,
            BodyTemplate = request.BodyTemplate,
            Channel = request.Channel,
        };

        NotificationTemplate created = await _service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    /// <summary>
    ///     Updates an existing notification template.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="request">The update request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with the updated template, or 404 if not found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<NotificationTemplate>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        UpdateNotificationTemplateCommand command = new()
        {
            TemplateKey = request.TemplateKey,
            TitleTemplate = request.TitleTemplate,
            BodyTemplate = request.BodyTemplate,
            Channel = request.Channel,
        };

        NotificationTemplate? updated = await _service.UpdateAsync(id, command, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    ///     Deletes a notification template.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
