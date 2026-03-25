using Hammer.Support.Infrastructure.Configuration;

namespace Hammer.Support.Tests.Infrastructure.Configuration;

public sealed class EnvFileLoaderTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public void Load_ValidKeyValuePairs_SetsEnvironmentVariables()
    {
        File.WriteAllText(_tempFile, "FOO_TEST_KEY=bar_value\nBAZ_TEST_KEY=qux_value");

        EnvFileLoader.Load(_tempFile);

        Assert.Equal("bar_value", Environment.GetEnvironmentVariable("FOO_TEST_KEY"));
        Assert.Equal("qux_value", Environment.GetEnvironmentVariable("BAZ_TEST_KEY"));

        Environment.SetEnvironmentVariable("FOO_TEST_KEY", null);
        Environment.SetEnvironmentVariable("BAZ_TEST_KEY", null);
    }

    [Fact]
    public void Load_SkipsCommentsAndBlankLines()
    {
        File.WriteAllText(_tempFile, "# comment\n\n  \nVALID_TEST_KEY=value");

        EnvFileLoader.Load(_tempFile);

        Assert.Equal("value", Environment.GetEnvironmentVariable("VALID_TEST_KEY"));

        Environment.SetEnvironmentVariable("VALID_TEST_KEY", null);
    }

    [Fact]
    public void Load_SkipsLinesWithoutSeparator()
    {
        File.WriteAllText(_tempFile, "NO_EQUALS_SIGN\n=no_key\nGOOD_TEST_KEY=good");

        EnvFileLoader.Load(_tempFile);

        Assert.Equal("good", Environment.GetEnvironmentVariable("GOOD_TEST_KEY"));

        Environment.SetEnvironmentVariable("GOOD_TEST_KEY", null);
    }

    [Fact]
    public void Load_NonExistentFile_DoesNotThrow()
    {
        Exception? exception = Record.Exception(() => EnvFileLoader.Load("/nonexistent/.env"));

        Assert.Null(exception);
    }

    [Fact]
    public void Load_ValueWithEquals_PreservesFullValue()
    {
        File.WriteAllText(_tempFile, "CONNECTION_TEST=host=localhost;port=5432");

        EnvFileLoader.Load(_tempFile);

        Assert.Equal("host=localhost;port=5432", Environment.GetEnvironmentVariable("CONNECTION_TEST"));

        Environment.SetEnvironmentVariable("CONNECTION_TEST", null);
    }
}
