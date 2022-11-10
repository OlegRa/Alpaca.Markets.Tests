namespace Alpaca.Markets.Tests;

public abstract class AlpacaDataClientBase<TClient> : IDisposable
    where TClient : IDisposable
{
    private readonly IAlpacaTradingClient _alpacaTradingClient;

    protected readonly String[] Symbols;

    protected readonly TClient Client;

    protected AlpacaDataClientBase(
        IAlpacaTradingClient alpacaTradingClient,
        TClient client,
        params String[] symbols)
    {
        _alpacaTradingClient = alpacaTradingClient;
        Symbols = symbols;
        Client = client;
    }

    public void Dispose()
    {
        Client.Dispose();
        _alpacaTradingClient.Dispose();
    }

    protected String Symbol => Symbols[0];

    protected async Task<DateTime> GetLastTradingDayCloseTimeUtc()
    {
        var calendars = await _alpacaTradingClient
            .ListIntervalCalendarAsync(new CalendarRequest().WithInterval(
                new Interval<DateOnly>(
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-14)),
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)))));

        Assert.NotNull(calendars);

        return calendars.Last().GetTradingCloseTimeUtc();
    }

    protected static void AssertPageIsValid<TItem>(
        IPage<TItem> page, Action<TItem> assertItemIsValid,
        Boolean isSinglePageExpected = true)
    {
        Assert.NotNull(page);
        Assert.NotNull(page.Items);
        Assert.NotEmpty(page.Items);

        if (isSinglePageExpected)
        {
            Assert.Null(page.NextPageToken);
        }
        else
        {
            Assert.NotNull(page.NextPageToken);
        }

        Assert.All(page.Items, assertItemIsValid);
    }

    protected static void AssertPageIsValid<TItem>(
        IMultiPage<TItem> page, Action<TItem, String> assertItemIsValid,
        Boolean isSinglePageExpected = true)
    {
        Assert.NotNull(page);
        Assert.NotNull(page.Items);
        Assert.NotEmpty(page.Items);

        if (isSinglePageExpected)
        {
            Assert.Null(page.NextPageToken);
        }
        else
        {
            Assert.NotNull(page.NextPageToken);
        }

        foreach (var kvp in page.Items)
        {
            Assert.All(kvp.Value, _ => assertItemIsValid(_, kvp.Key));
        }
    }

    protected void AssertBarIsValid(IBar bar) => AssertBarIsValid(bar, Symbol);

    protected void AssertBarIsValid(IBar bar, String symbol)
    {
        Assert.NotNull(bar);
        Assert.Equal(symbol, bar.Symbol);

        Assert.True(bar.Open >= 0M);
        Assert.True(bar.Close >= 0M);
        Assert.True(bar.High >= 0M);
        Assert.True(bar.Low >= 0M);

        Assert.True(bar.Low <= bar.High);
        Assert.InRange(bar.Open, bar.Low, bar.High);
        Assert.InRange(bar.Close, bar.Low, bar.High);

        Assert.True(bar.Vwap >= 0UL);
        Assert.True(bar.Volume >= 0UL);
        Assert.True(bar.TradeCount != 0UL);

        Assert.True(bar.TimeUtc <= DateTime.UtcNow);
    }

    protected void AssertQuoteIsValid(IQuote quote) => AssertQuoteIsValid(quote, Symbol);

    protected void AssertQuoteIsValid(IQuote quote, String symbol)
    {
        Assert.NotNull(quote);
        Assert.Equal(symbol, quote.Symbol);

        Assert.NotNull(quote.Tape);
        Assert.NotNull(quote.Conditions);
        Assert.NotNull(quote.AskExchange);
        Assert.NotNull(quote.BidExchange);

        Assert.True(quote.AskSize >= 0M);
        Assert.True(quote.BidSize >= 0M);
        Assert.True(quote.AskPrice >= 0M);
        Assert.True(quote.BidPrice >= 0M);

        Assert.True(quote.TimestampUtc <= DateTime.UtcNow);
    }

    protected void AssertTradeIsValid(ITrade trade) => AssertTradeIsValid(trade, Symbol);

    protected void AssertTradeIsValid(ITrade trade, String symbol)
    {
        Assert.NotNull(trade);
        Assert.Equal(symbol, trade.Symbol);

        Assert.NotNull(trade.Tape);
        Assert.NotNull(trade.Exchange);
        Assert.NotNull(trade.Conditions);

        Assert.True(trade.Size >= 0M);
        Assert.True(trade.Price >= 0M);

        Assert.NotEqual(trade.TakerSide == TakerSide.Unknown ? 0UL : UInt64.MaxValue, trade.TradeId);

        Assert.True(trade.TimestampUtc <= DateTime.UtcNow);
    }
}
