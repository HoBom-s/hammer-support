using FluentAssertions;
using Hammer.Support.Infrastructure.Notification;

namespace Hammer.Support.Tests.Infrastructure.Notification;

public sealed class TemplateRendererTests
{
    [Fact]
    public void Render_SubstitutesPlaceholders()
    {
        var template = "Hello {name}, your order #{orderId} is ready.";
        var variables = new Dictionary<string, string>
        {
            ["name"] = "Fox",
            ["orderId"] = "42",
        };

        var result = TemplateRenderer.Render(template, variables);

        result.Should().Be("Hello Fox, your order #42 is ready.");
    }

    [Fact]
    public void Render_NoVariables_ReturnsTemplateUnchanged()
    {
        var template = "No placeholders here.";

        var result = TemplateRenderer.Render(template, new Dictionary<string, string>());

        result.Should().Be("No placeholders here.");
    }

    [Fact]
    public void Render_UnmatchedPlaceholder_LeavesItInPlace()
    {
        var template = "Hello {name}, {missing} stays.";
        var variables = new Dictionary<string, string> { ["name"] = "Fox" };

        var result = TemplateRenderer.Render(template, variables);

        result.Should().Be("Hello Fox, {missing} stays.");
    }

    [Fact]
    public void Render_MultipleSamePlaceholder_ReplacesAll()
    {
        var template = "{item} costs {price}. Buy {item} now!";
        var variables = new Dictionary<string, string>
        {
            ["item"] = "Widget",
            ["price"] = "$10",
        };

        var result = TemplateRenderer.Render(template, variables);

        result.Should().Be("Widget costs $10. Buy Widget now!");
    }

    [Fact]
    public void Render_EmptyValueVariable_ReplacesWithEmpty()
    {
        var template = "Value: {val}";
        var variables = new Dictionary<string, string> { ["val"] = string.Empty };

        var result = TemplateRenderer.Render(template, variables);

        result.Should().Be("Value: ");
    }

    [Fact]
    public void RenderWithUnmatched_AllResolved_ReturnsEmptyList()
    {
        var template = "Hello {name}";
        var variables = new Dictionary<string, string> { ["name"] = "Fox" };

        TemplateRenderer.Render(template, variables, out IReadOnlyList<string> unmatched);

        unmatched.Should().BeEmpty();
    }

    [Fact]
    public void RenderWithUnmatched_MissingVariable_ReportsPlaceholder()
    {
        var template = "Hello {name}, {missing} here.";
        var variables = new Dictionary<string, string> { ["name"] = "Fox" };

        var result = TemplateRenderer.Render(template, variables, out IReadOnlyList<string> unmatched);

        result.Should().Be("Hello Fox, {missing} here.");
        unmatched.Should().ContainSingle().Which.Should().Be("missing");
    }

    [Fact]
    public void RenderWithUnmatched_MultipleUnmatched_ReportsAll()
    {
        var template = "{a} and {b}";
        var variables = new Dictionary<string, string>();

        TemplateRenderer.Render(template, variables, out IReadOnlyList<string> unmatched);

        unmatched.Should().HaveCount(2);
        unmatched.Should().Contain("a");
        unmatched.Should().Contain("b");
    }
}
