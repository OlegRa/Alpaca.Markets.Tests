#if EXTENSIONS

using System.Net;
using Alpaca.Markets.Extensions;

namespace Alpaca.Markets.Tests;

public sealed partial class AlpacaCryptoDataClientTest
{
    [Fact]
    public async void GetHistoricalBarsAsAsyncEnumerableWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        await foreach (var bar in Client
                           .GetHistoricalBarsAsAsyncEnumerable(
                               new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Hour)))
        {
            Assert.NotNull(bar);
            Assert.InRange(bar.TimeUtc, from, into);
        }
    }

    [Fact]
    public async void GetHistoricalQuotesAsAsyncEnumerableWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-3).Date;

        var count = 0;
        var cancellationTokenSource = new CancellationTokenSource(
            TimeSpan.FromSeconds(15));
        try
        {
            await foreach (var quote in Client
               .GetHistoricalQuotesAsAsyncEnumerable(
                   new HistoricalCryptoQuotesRequest(Symbol, from, into), cancellationTokenSource.Token))
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
