namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
public sealed partial class AlpacaCryptoDataClientTest : IDisposable
{
    private const String Symbol = "BTCUSD";

    private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

    private readonly IAlpacaCryptoDataClient _alpacaCryptoDataClient;

    public AlpacaCryptoDataClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
    {
        _clientsFactory = clientsFactory;
        _alpacaCryptoDataClient = clientsFactory.GetAlpacaCryptoDataClient();
    }

    [Fact]
    public async void ListDayHistoricalBarsWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await _alpacaCryptoDataClient.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Day));

        Assert.NotNull(bars);
        Assert.NotNull(bars.Items);
        Assert.NotEmpty(bars.Items);
    }

    [Fact]
    public async void ListHourHistoricalBarsWorks()
    {
        var into = await getLastTradingDay();
        var from = into.AddHours(-5);
        var bars = await _alpacaCryptoDataClient.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Hour));

        Assert.NotNull(bars);
        Assert.NotNull(bars.Items);
        Assert.NotEmpty(bars.Items);
    }

    [Fact]
    public async void ListMinuteHistoricalBarsWorks()
    {
        var into = await getLastTradingDay();
        var from = into.AddMinutes(-25);
        var bars = await _alpacaCryptoDataClient.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Minute));

        Assert.NotNull(bars);
        Assert.NotNull(bars.Items);
        Assert.NotEmpty(bars.Items);
    }

    [Fact]
    public async void ListHistoricalQuotesWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-3).Date;
        var quotes = await _alpacaCryptoDataClient.ListHistoricalQuotesAsync(
            new HistoricalCryptoQuotesRequest(Symbol, from, into));

        Assert.NotNull(quotes);
        Assert.NotNull(quotes.Items);
        Assert.NotEmpty(quotes.Items);
    }

    [Fact]
    public async void ListHistoricalTradesWorks()
    {
        var into = (await getLastTradingDay()).Date;
        var from = into.AddDays(-3).Date;
        var quotes = await _alpacaCryptoDataClient.ListHistoricalTradesAsync(
            new HistoricalCryptoTradesRequest(Symbol, from, into));

        Assert.NotNull(quotes);
        Assert.NotNull(quotes.Items);
        Assert.NotEmpty(quotes.Items);
    }

    [Fact]
    public async void GetLatestQuoteWorks()
    {
        var quote = await _alpacaCryptoDataClient.GetLatestQuoteAsync(
            new LatestDataRequest(Symbol, CryptoExchange.Cbse));

        Assert.NotNull(quote);
        Assert.Equal(Symbol, quote.Symbol);
    }

    [Fact]
    public async void GetLatestTradeWorks()
    {
        var trade = await _alpacaCryptoDataClient.GetLatestTradeAsync(
            new LatestDataRequest(Symbol, CryptoExchange.Ersx));

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

    public void Dispose() => _alpacaCryptoDataClient?.Dispose();
}
