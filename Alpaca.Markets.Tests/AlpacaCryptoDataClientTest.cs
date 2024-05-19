namespace Alpaca.Markets.Tests;

[Collection(nameof(PaperEnvironmentClientsFactoryCollection))]
// ReSharper disable once PartialTypeWithSinglePart
public sealed partial class AlpacaCryptoDataClientTest : AlpacaDataClientBase<IAlpacaCryptoDataClient>
{
    public AlpacaCryptoDataClientTest(
        PaperEnvironmentClientsFactoryFixture clientsFactory)
        : base(
            clientsFactory.GetAlpacaTradingClient(),
            clientsFactory.GetAlpacaCryptoDataClient(),
            "BTC/USD", "ETH/USD")
    {
    }

    [Fact]
    public async Task ListDayHistoricalBarsWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Day));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task ListHourHistoricalBarsWorks()
    {
        var into = await GetLastTradingDayCloseTimeUtc();
        var from = into.AddHours(-5);
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Hour));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task ListMinuteHistoricalBarsWorks()
    {
        var into = await GetLastTradingDayCloseTimeUtc();
        var from = into.AddMinutes(-25);
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Minute));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task GetDayHistoricalBarsWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await Client.GetHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbols, from, into, BarTimeFrame.Day));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async Task ListHistoricalTradesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-1).Date;
        var trades = await Client.ListHistoricalTradesAsync(
            new HistoricalCryptoTradesRequest(Symbol, from, into).WithPageSize(10));

        AssertPageIsValid(trades, AssertTradeIsValid);
    }

    [Fact]
    public async Task GetHistoricalTradesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-1).Date;
        var trades = await Client.GetHistoricalTradesAsync(
            new HistoricalCryptoTradesRequest(Symbols, from, into).WithPageSize(10));

        AssertPageIsValid(trades, AssertTradeIsValid);
    }

    [Fact]
    public async Task ListLatestQuotesWorks()
    {
        foreach (var kvp in await Client
                     .ListLatestQuotesAsync(new LatestDataListRequest(Symbols)))
        {
            AssertQuoteIsValid(kvp.Value, kvp.Key);
        }
    }

    [Fact]
    public async Task ListLatestTradesWorks()
    {
        foreach (var kvp in await Client
                     .ListLatestTradesAsync(new LatestDataListRequest(Symbols)))
        {
            AssertTradeIsValid(kvp.Value, kvp.Key);
        }
    }

    [Fact]
    public async Task LatestDataListRequestValidationWorks() =>
        await Assert.ThrowsAsync<RequestValidationException>(() => Client.ListLatestTradesAsync(
            new LatestDataListRequest(new []{ String.Empty })));

    [Fact]
    public async Task GetSnapshotWorks()
    {
        var snapshots = await Client.ListSnapshotsAsync(
            new SnapshotDataListRequest(Symbols));

        Assert.NotNull(snapshots);

        foreach (var kvp in snapshots)
        {
            assertSnapshotIsValid(kvp.Value, kvp.Key);
        }
    }

    [Fact]
    public async Task SnapshotDataListRequestValidationWorks() =>
        await Assert.ThrowsAsync<RequestValidationException>(() => Client
            .ListSnapshotsAsync(new SnapshotDataListRequest(new []{ String.Empty })));

    private void assertSnapshotIsValid(ISnapshot snapshot, String symbol)
    {
        AssertBarIsValid(snapshot.PreviousDailyBar!, symbol);
        AssertBarIsValid(snapshot.CurrentDailyBar!, symbol);
        AssertBarIsValid(snapshot.MinuteBar!, symbol);

        AssertQuoteIsValid(snapshot.Quote!, symbol);
        AssertTradeIsValid(snapshot.Trade!, symbol);
    }
}
