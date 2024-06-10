namespace Alpaca.Markets.Tests;

[Collection(nameof(PaperEnvironmentClientsFactoryCollection))]
// ReSharper disable once PartialTypeWithSinglePart
public sealed partial class AlpacaDataClientTest : AlpacaDataClientBase<IAlpacaDataClient>
{
    public AlpacaDataClientTest(
        PaperEnvironmentClientsFactoryFixture clientsFactory)
        : base(
            clientsFactory.GetAlpacaTradingClient(),
            clientsFactory.GetAlpacaDataClient(),
            "AAPL", "MSFT")
    {
    }

    [Fact]
    public async Task ListDayHistoricalBarsWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Day));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task ListHourHistoricalBarsWorks()
    {
        var into = await GetLastTradingDayCloseTimeUtc();
        var from = into.AddHours(-5);
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Hour));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task ListMinuteHistoricalBarsWorks()
    {
        var into = await GetLastTradingDayCloseTimeUtc();
        var from = into.AddMinutes(-25);
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Minute));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task GetDayHistoricalBarsWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await Client.GetHistoricalBarsAsync(
            new HistoricalBarsRequest(Symbols, from, into, BarTimeFrame.Day));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task ListHistoricalQuotesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var quotes = await Client.ListHistoricalQuotesAsync(
            new HistoricalQuotesRequest(Symbol, from, into));

        AssertPageIsValid(quotes, AssertQuoteIsValid, false);
    }

    [Fact]
    public async Task GetHistoricalQuotesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var quotes = await Client.GetHistoricalQuotesAsync(
            new HistoricalQuotesRequest(Symbols, from, into));

        AssertPageIsValid(quotes, AssertQuoteIsValid, false);
    }

    [Fact]
    public async Task ListHistoricalTradesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var trades = await Client.ListHistoricalTradesAsync(
            new HistoricalTradesRequest(Symbol, from, into));

        AssertPageIsValid(trades, AssertTradeIsValid, false);
    }

    [Fact]
    public async Task GetHistoricalTradesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var trades = await Client.GetHistoricalTradesAsync(
            new HistoricalTradesRequest(Symbols, from, into));

        AssertPageIsValid(trades, AssertTradeIsValid, false);
    }

    [Fact]
    public async Task GetLatestQuoteWorks() =>
        AssertQuoteIsValid(await Client.GetLatestQuoteAsync(new LatestMarketDataRequest(Symbol)));

    [Fact]
    public async Task GetLatestTradeWorks() =>
        AssertTradeIsValid(await Client.GetLatestTradeAsync(new LatestMarketDataRequest(Symbol)));

    [Fact]
    public async Task GetSnapshotWorks()
    {
        var snapshot = await Client.GetSnapshotAsync(new LatestMarketDataRequest(Symbol));

        Assert.NotNull(snapshot);
        Assert.Equal(Symbol, snapshot.Symbol);

        assertSnapshotIsValid(snapshot, Symbol);
    }

    [Fact]
    public async Task GetSnapshotsWorks()
    {
        var snapshotsDictionary = await Client.ListSnapshotsAsync(new LatestMarketDataListRequest(Symbols));

        Assert.NotNull(snapshotsDictionary);

        // ReSharper disable once UseDeconstruction
        foreach (var kvp in snapshotsDictionary)
        {
            assertSnapshotIsValid(kvp.Value, kvp.Key);
        }
    }

    [Fact]
    public async Task ListExchangesWorks() => 
        assertStringDictionaryIsValid(await Client.ListExchangesAsync());

    [Fact]
    public async Task ListTradeConditionsWorks() => 
        assertStringDictionaryIsValid(await Client.ListTradeConditionsAsync(Tape.A));

    [Fact]
    public async Task ListQuoteConditionsWorks() => 
        assertStringDictionaryIsValid(await Client.ListQuoteConditionsAsync(Tape.C));

    private void assertSnapshotIsValid(ISnapshot snapshot, String symbol)
    {
        AssertBarIsValid(snapshot.PreviousDailyBar!, symbol);
        AssertBarIsValid(snapshot.CurrentDailyBar!, symbol);
        AssertBarIsValid(snapshot.MinuteBar!, symbol);

        AssertQuoteIsValid(snapshot.Quote!, symbol);
        AssertTradeIsValid(snapshot.Trade!, symbol);
    }

    private static void assertStringDictionaryIsValid(IReadOnlyDictionary<String, String> exchanges)
    {
        // ReSharper disable once UseDeconstruction
        foreach (var kvp in exchanges)
        {
            Assert.NotNull(kvp.Key);
            Assert.False(String.IsNullOrWhiteSpace(kvp.Value));
        }
    }}
