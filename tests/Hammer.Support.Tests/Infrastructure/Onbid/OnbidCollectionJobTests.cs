using Hammer.Support.Infrastructure.Onbid;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

public sealed class OnbidCollectionJobTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(23)]
    public void CalculateDelayUntilNextRun_ReturnsPositiveDelayWithin24Hours(int targetHour)
    {
        TimeSpan delay = OnbidCollectionJob.CalculateDelayUntilNextRun(targetHour);

        Assert.True(delay > TimeSpan.Zero);
        Assert.True(delay <= TimeSpan.FromHours(24));
    }
}
