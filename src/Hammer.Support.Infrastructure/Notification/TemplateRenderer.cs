using System.Text.RegularExpressions;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
///     Renders notification templates by substituting {placeholder} variables.
/// </summary>
internal static partial class TemplateRenderer
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

    /// <summary>
    ///     Replaces all {key} placeholders and reports any unmatched placeholders remaining.
    /// </summary>
    /// <param name="template">Template string with {placeholder} markers.</param>
    /// <param name="variables">Key-value pairs for substitution.</param>
    /// <param name="unmatchedPlaceholders">Placeholder names that were not substituted.</param>
    /// <returns>Rendered string.</returns>
    public static string Render(
        string template,
        IReadOnlyDictionary<string, string> variables,
        out IReadOnlyList<string> unmatchedPlaceholders)
    {
        var rendered = Render(template, variables);

        unmatchedPlaceholders = PlaceholderPattern()
            .Matches(rendered)
            .Select(m => m.Groups[1].Value)
            .ToList();

        return rendered;
    }

    [GeneratedRegex(@"\{(\w+)\}", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PlaceholderPattern();
}
