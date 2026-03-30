namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
///     Renders notification templates by substituting {placeholder} variables.
/// </summary>
internal static class TemplateRenderer
{
    /// <summary>
    ///     Replaces all {key} placeholders in the template with corresponding values.
    /// </summary>
    /// <param name="template">Template string with {placeholder} markers.</param>
    /// <param name="variables">Key-value pairs for substitution.</param>
    /// <returns>Rendered string.</returns>
    public static string Render(string template, IReadOnlyDictionary<string, string> variables) =>
        variables.Aggregate(
            template,
            (current, pair) => current.Replace($"{{{pair.Key}}}", pair.Value, StringComparison.Ordinal));
}
