#if EXTENSIONS

using System.Net;
using Alpaca.Markets.Extensions;

namespace Alpaca.Markets.Tests;

public sealed partial class AlpacaDataClientTest
{
    [Fact]
    public async void GetHistoricalBarsAsAsyncEnumerableWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-5).Date;
        await foreach (var bar in _alpacaDataClient
                           .GetHistoricalBarsAsAsyncEnumerable(
                               new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Hour)))
        {
            Assert.NotNull(bar);
            Assert.InRange(bar.TimeUtc, from, into);
        }
    }

    [Fact]
    public async void GetHistoricalQuotesAsAsyncEnumerableWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-3).Date;

        var count = 0;
        var cancellationTokenSource = new CancellationTokenSource(
            TimeSpan.FromSeconds(90));
        try
        {
            await foreach (var quote in _alpacaDataClient
               .GetHistoricalQuotesAsAsyncEnumerable(
                   new HistoricalQuotesRequest(Symbol, from, into), cancellationTokenSource.Token))
            {
                Assert.NotNull(quote);
                Assert.InRange(quote.TimestampUtc, from, into);
                ++count;
            }
        }
        catch (OperationCanceledException)
        {
            Assert.True(cancellationTokenSource.IsCancellationRequested);
        }
        catch (WebException)
        {
            Assert.True(cancellationTokenSource.IsCancellationRequested);
        }

        Assert.NotEqual(0, count);
    }
}

#endif
