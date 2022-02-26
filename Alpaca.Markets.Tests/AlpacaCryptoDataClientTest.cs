namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
// ReSharper disable once PartialTypeWithSinglePart
public sealed partial class AlpacaCryptoDataClientTest : AlpacaDataClientBase<IAlpacaCryptoDataClient>
{
    public AlpacaCryptoDataClientTest(
        PaperEnvironmentClientsFactoryFixture clientsFactory)
        : base(
            clientsFactory.GetAlpacaTradingClient(),
            clientsFactory.GetAlpacaCryptoDataClient(),
            "BTCUSD", "ETHUSD")
    {
    }

    [Fact]
    public async void ListDayHistoricalBarsWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Day));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async void ListHourHistoricalBarsWorks()
    {
        var into = await GetLastTradingDayCloseTimeUtc();
        var from = into.AddHours(-5);
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Hour));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async void ListMinuteHistoricalBarsWorks()
    {
        var into = await GetLastTradingDayCloseTimeUtc();
        var from = into.AddMinutes(-25);
        var bars = await Client.ListHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbol, from, into, BarTimeFrame.Minute));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async void GetDayHistoricalBarsWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-5).Date;
        var bars = await Client.GetHistoricalBarsAsync(
            new HistoricalCryptoBarsRequest(Symbols, from, into, BarTimeFrame.Day));

        AssertPageIsValid(bars, AssertBarIsValid);
    }

    [Fact]
    public async void ListHistoricalQuotesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-3).Date;
        var quotes = await Client.ListHistoricalQuotesAsync(
            new HistoricalCryptoQuotesRequest(Symbol, from, into));

        AssertPageIsValid(quotes, AssertQuoteIsValid, false);
    }

    [Fact]
    public async void GetHistoricalQuotesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-3).Date;
        var quotes = await Client.GetHistoricalQuotesAsync(
            new HistoricalCryptoQuotesRequest(Symbols, from, into));

        AssertPageIsValid(quotes, AssertQuoteIsValid, false);
    }

    [Fact]
    public async void ListHistoricalTradesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-3).Date;
        var trades = await Client.ListHistoricalTradesAsync(
            new HistoricalCryptoTradesRequest(Symbol, from, into));

        AssertPageIsValid(trades, AssertTradeIsValid, false);
    }

    [Fact]
    public async void GetHistoricalTradesWorks()
    {
        var into = (await GetLastTradingDayCloseTimeUtc()).Date;
        var from = into.AddDays(-3).Date;
        var trades = await Client.GetHistoricalTradesAsync(
            new HistoricalCryptoTradesRequest(Symbols, from, into));

        AssertPageIsValid(trades, AssertTradeIsValid, false);
    }

    [Fact]
    public async void GetLatestQuoteWorks() =>
        AssertQuoteIsValid(await Client.GetLatestQuoteAsync(
            new LatestDataRequest(Symbol, CryptoExchange.Cbse)));

    [Fact]
    public async void GetLatestTradeWorks() =>
        AssertTradeIsValid(await Client.GetLatestTradeAsync(
            new LatestDataRequest(Symbol, CryptoExchange.Ersx)));

    [Fact(Skip = "Re-enable it after releasing SDK with updated sources.")]
    public async void LatestTradeRequestValidationWorks() =>
        await Assert.ThrowsAsync<RequestValidationException>(() => Client.GetLatestTradeAsync(
            new LatestDataRequest(String.Empty, CryptoExchange.Ersx)));

    [Fact]
    public async void GetSnapshotWorks()
    {
        var snapshot = await Client.GetSnapshotAsync(
            new SnapshotDataRequest(Symbol, CryptoExchange.Cbse));

        Assert.NotNull(snapshot);
        Assert.Equal(Symbol, snapshot.Symbol);

        assertSnapshotIsValid(snapshot, Symbol);
    }

    [Fact(Skip = "Re-enable it after releasing SDK with updated sources.")]
    public async void SnapshotDataRequestValidationWorks() =>
        await Assert.ThrowsAsync<RequestValidationException>(() => Client
            .GetSnapshotAsync(new SnapshotDataRequest(String.Empty, CryptoExchange.Cbse)));

    [Fact]
    public async void GetLatestBestBidOfferWorks() =>
        AssertQuoteIsValid(await Client.GetLatestBestBidOfferAsync(new LatestBestBidOfferRequest(Symbol)));

    private void assertSnapshotIsValid(ISnapshot snapshot, String symbol)
    {
        AssertBarIsValid(snapshot.PreviousDailyBar!, symbol);
        AssertBarIsValid(snapshot.CurrentDailyBar!, symbol);
        AssertBarIsValid(snapshot.MinuteBar!, symbol);

        AssertQuoteIsValid(snapshot.Quote!, symbol);
        AssertTradeIsValid(snapshot.Trade!, symbol);
    }
}
