﻿namespace Alpaca.Markets.Tests;

public sealed class IntervalTest
{
    [Fact]
    public void IntervalConversionsWorks()
    {
        var dateOnlyInterval = DateOnly.FromDateTime(DateTime.Today).GetIntervalFromThat();
        Assert.NotNull(dateOnlyInterval.From);
        Assert.Null(dateOnlyInterval.Into);

        var dateTimeInterval = dateOnlyInterval.AsTimeInterval();
        Assert.NotNull(dateTimeInterval.From);
        Assert.Null(dateTimeInterval.Into);

        Assert.Equal(dateOnlyInterval.From!.Value.ToDateTime(TimeOnly.MinValue), dateTimeInterval.From!.Value);
        Assert.Equal(dateOnlyInterval, dateTimeInterval.AsDateInterval());
    }
}
