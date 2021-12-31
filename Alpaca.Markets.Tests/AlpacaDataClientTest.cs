using NuGet.Frameworks;

namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
public sealed partial class AlpacaDataClientTest : IDisposable
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
        var quote = await _alpacaDataClient.GetLatestQuoteAsync(Symbol);

        Assert.NotNull(quote);
        Assert.Equal(Symbol, quote.Symbol);
        Assert.True(quote.TimestampUtc <= DateTime.UtcNow);

        Assert.NotNull(quote.Tape);
        Assert.NotNull(quote.Conditions);
        Assert.NotNull(quote.AskExchange);
        Assert.NotNull(quote.BidExchange);

        Assert.NotEqual(0, quote.AskPrice);
        Assert.NotEqual(0, quote.BidPrice);
        Assert.NotEqual(0, quote.AskSize);
        Assert.NotEqual(0, quote.BidSize);
    }

    [Fact]
    public async void GetLatestTradeWorks()
    {
        var trade = await _alpacaDataClient.GetLatestTradeAsync(Symbol);

        Assert.NotNull(trade);
        Assert.Equal(Symbol, trade.Symbol);
    }

    private async Task<DateTime> getLastTradingDay()
    {
        using var alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();

        var calendars = await alpacaTradingClient
            .ListCalendarAsync(new CalendarRequest().WithInterval(
                new Interval<DateOnly>(
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-14)),
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)))));

        Assert.NotNull(calendars);

        return calendars.Last().TradingCloseTimeUtc;
    }

    public void Dispose() => _alpacaDataClient?.Dispose();
}
