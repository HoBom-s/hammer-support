namespace Hammer.Support.Infrastructure.Configuration;

/// <summary>
///     Loads environment variables from .env files.
/// </summary>
public static class EnvFileLoader
{
    /// <summary>
    ///     Reads a .env file and sets each key=value pair as an environment variable.
    ///     Lines starting with # and blank lines are ignored.
    /// </summary>
    /// <param name="filePath">Path to the .env file.</param>
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();

            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var separatorIndex = trimmed.IndexOf('=', StringComparison.Ordinal);

            if (separatorIndex <= 0)
                continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
