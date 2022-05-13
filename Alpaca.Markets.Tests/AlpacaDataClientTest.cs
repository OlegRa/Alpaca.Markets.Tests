using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alpaca.Markets.Extensions;
using Xunit;

namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
public sealed class AlpacaDataClientTest : IDisposable
{
    private const String Symbol = "AAPL";

    private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

    private readonly IAlpacaDataClient _alpacaDataClient;

    public AlpacaDataClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
    {
        _clientsFactory = clientsFactory;
        _alpacaDataClient = clientsFactory.GetAlpacaDataClient();
    }

    [Fact]
    public async void ListDayHistoricalBarsWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await _alpacaDataClient.ListHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Day));

        Assert.NotNull(bars);
        Assert.NotNull(bars.Items);
        Assert.NotEmpty(bars.Items);
    }

    [Fact]
    public async void ListHourHistoricalBarsWorks()
    {
        var into = await getLastTradingDay();
        var from = into.AddHours(-5);
        var bars = await _alpacaDataClient.ListHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Hour));

        Assert.NotNull(bars);
        Assert.NotNull(bars.Items);
        Assert.NotEmpty(bars.Items);
    }

    [Fact]
    public async void ListMinuteHistoricalBarsWorks()
    {
        var into = await getLastTradingDay();
        var from = into.AddMinutes(-25);
        var bars = await _alpacaDataClient.ListHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Minute));

        Assert.NotNull(bars);
        Assert.NotNull(bars.Items);
        Assert.NotEmpty(bars.Items);
    }

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
    public async void ListHistoricalQuotesWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-3).Date;
        var quotes = await _alpacaDataClient.ListHistoricalQuotesAsync(
            new HistoricalQuotesRequest(Symbol, from, into));

        Assert.NotNull(quotes);
        Assert.NotNull(quotes.Items);
        Assert.NotEmpty(quotes.Items);
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
                               // ReSharper disable once MethodSupportsCancellation
                               .GetHistoricalQuotesAsAsyncEnumerable(
                                   new HistoricalQuotesRequest(Symbol, from, into))
                               .WithCancellation(cancellationTokenSource.Token))
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

    [Fact]
    public async void ListHistoricalTradesWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-3).Date;
        var quotes = await _alpacaDataClient.ListHistoricalTradesAsync(
            new HistoricalTradesRequest(Symbol, from, into));

        Assert.NotNull(quotes);
        Assert.NotNull(quotes.Items);
        Assert.NotEmpty(quotes.Items);
    }

    [Fact]
    public async void GetLatestQuoteWorks()
    {
        var quote = await _alpacaDataClient.GetLatestQuoteAsync(new LatestMarketDataRequest(Symbol));

        Assert.NotNull(quote);
        Assert.Equal(Symbol, quote.Symbol);
    }

    [Fact]
    public async void GetLatestTradeWorks()
    {
        var trade = await _alpacaDataClient.GetLatestTradeAsync(new LatestMarketDataRequest(Symbol));

        Assert.NotNull(trade);
        Assert.Equal(Symbol, trade.Symbol);
    }

    private async Task<DateTime> getLastTradingDay()
    {
        using var alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();

        var calendars = await alpacaTradingClient
            .ListCalendarAsync(new CalendarRequest()
                .SetInclusiveTimeInterval(
                    DateTime.UtcNow.Date.AddDays(-14),
                    DateTime.UtcNow.Date.AddDays(-1)));

        Assert.NotNull(calendars);

        return calendars.Last().TradingCloseTimeUtc;
    }

    public void Dispose() => _alpacaDataClient?.Dispose();
}
