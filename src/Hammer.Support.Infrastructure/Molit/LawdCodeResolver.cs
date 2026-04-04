using System.Reflection;
using Hammer.Support.Application.Abstractions;

namespace Hammer.Support.Infrastructure.Molit;

/// <summary>
/// Resolves Korean addresses to 5-digit LAWD_CD (법정동 시군구 코드)
/// using an embedded TSV lookup table.
/// </summary>
public sealed class LawdCodeResolver : ILawdCodeResolver
{
    private const string ResourceName = "Hammer.Support.Infrastructure.Molit.Data.lawd_code.tsv";

    /// <summary>
    /// Ordered entries for prefix matching (longest name first).
    /// </summary>
    private readonly (string Name, string Code)[] _entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="LawdCodeResolver"/> class.
    /// Loads the embedded lawd_code.tsv resource.
    /// </summary>
    public LawdCodeResolver()
    {
        _entries = LoadEntries();
    }

    /// <inheritdoc />
    public string? Resolve(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        return _entries
            .Where(entry => address.StartsWith(entry.Name, StringComparison.Ordinal))
            .Select(entry => entry.Code)
            .FirstOrDefault();
    }

    private static (string Name, string Code)[] LoadEntries()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

        using StreamReader reader = new(stream);
        List<(string Name, string Code)> entries = [];

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split('\t', 2);
            if (parts.Length == 2)
                entries.Add((parts[1].Trim(), parts[0].Trim()));
        }

        // Sort by name length descending so longer prefixes match first.
        // e.g. "경기도 성남시 수정구" matches before "경기도 성남시".
        entries.Sort((a, b) => b.Name.Length.CompareTo(a.Name.Length));

        return [.. entries];
    }
}
