namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
public sealed partial class AlpacaTradingClientTest : IDisposable
{
    private const String Symbol = "AAPL";

    private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

    private readonly IAlpacaTradingClient _alpacaTradingClient;

    public AlpacaTradingClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
    {
        _clientsFactory = clientsFactory;
        _alpacaTradingClient = clientsFactory.GetAlpacaTradingClient();
    }

    [Fact]
    public async void GetPortfolioHistoryAsyncWorks()
    {
        var portfolioHistory = await _alpacaTradingClient.GetPortfolioHistoryAsync(new PortfolioHistoryRequest());

        Assert.NotNull(portfolioHistory);
        Assert.NotNull(portfolioHistory.Items);
        Assert.True(portfolioHistory.BaseValue >= 0M);

        var lastTimestamp = DateTime.MinValue;
        foreach (var item in portfolioHistory.Items)
        {
            Assert.NotNull(item.Equity);
            Assert.NotNull(item.ProfitLoss);
            Assert.NotNull(item.ProfitLossPercentage);
            
            Assert.True(lastTimestamp < item.TimestampUtc);
            lastTimestamp = item.TimestampUtc;
        }
    }

    [Fact]
    public async void GetAssetAsyncThrowsCustomException()
    {
        var exception = await Assert.ThrowsAsync<RestClientErrorException>(
            () => _alpacaTradingClient.GetAssetAsync("HEI-A"));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.NotEqual(0, exception.ErrorCode);
    }

    [Fact]
    public async void GetAccountWorks()
    {
        var account = await _alpacaTradingClient.GetAccountAsync();

        Assert.NotNull(account);
        Assert.Equal("USD", account.Currency);
    }

    [Fact]
    public async void GetAccountConfigurationWorks()
    {
        var accountConfiguration = await _alpacaTradingClient.GetAccountConfigurationAsync();

        Assert.NotNull(accountConfiguration);
        Assert.False(accountConfiguration.IsNoShorting);
    }

    [Fact]
    public async void PatchAccountConfigurationWorks()
    {
        var accountConfigurationOld = await _alpacaTradingClient.GetAccountConfigurationAsync();

        Assert.NotNull(accountConfigurationOld);

        accountConfigurationOld.TradeConfirmEmail =
            accountConfigurationOld.TradeConfirmEmail == TradeConfirmEmail.All
                ? TradeConfirmEmail.None
                : TradeConfirmEmail.All;

        var accountConfigurationNew = await _alpacaTradingClient.PatchAccountConfigurationAsync(accountConfigurationOld);

        Assert.NotNull(accountConfigurationNew);
        Assert.NotEqual(accountConfigurationNew, accountConfigurationOld);

        Assert.Equal(accountConfigurationOld.TradeConfirmEmail, accountConfigurationNew.TradeConfirmEmail);
    }

    [Fact]
    public async void ListOrdersWorks()
    {
        var orders = await _alpacaTradingClient.ListOrdersAsync(new ListOrdersRequest());

        Assert.NotNull(orders);
        // Assert.NotEmpty(orders);
    }

    [Fact]
    public async void GetOrderWorks()
    {
        var orders = await _alpacaTradingClient.ListOrdersAsync(
            new ListOrdersRequest { OrderStatusFilter = OrderStatusFilter.All });

        Assert.NotNull(orders);

        var ordersList = orders.ToList();
        Assert.NotEmpty(ordersList);
        var first = ordersList.First();

        var orderById = await _alpacaTradingClient.GetOrderAsync(first.OrderId);
        var orderByClientId = await _alpacaTradingClient.GetOrderAsync(
            first.ClientOrderId ?? String.Empty);

        Assert.NotNull(orderById);
        Assert.NotNull(orderByClientId);

        Assert.Equal(orderById.OrderId, orderByClientId.OrderId);
        Assert.Equal(orderById.ClientOrderId, orderByClientId.ClientOrderId);
    }

    [Fact]
    public async void ListPositionsWorks()
    {
        var positions = await _alpacaTradingClient.ListPositionsAsync();

        Assert.NotNull(positions);
        Assert.NotEmpty(positions);
    }

    [Fact]
    public async void GetPositionWorks()
    {
        var position = await _alpacaTradingClient.GetPositionAsync(Symbol);

        Assert.NotNull(position);
        Assert.Equal(Symbol, position.Symbol);
    }

    [Fact]
    public async void ListAssetsWorks()
    {
        var assets = await _alpacaTradingClient.ListAssetsAsync(
            new AssetsRequest { AssetClass = AssetClass.Crypto, AssetStatus = AssetStatus.Active});

        Assert.NotNull(assets);
        Assert.NotEmpty(assets);
    }

    [Fact]
    public async void GetAssetWorks()
    {
        var asset = await _alpacaTradingClient.GetAssetAsync(Symbol);

        Assert.NotNull(asset);
        Assert.Equal(Symbol, asset.Symbol);

        Assert.NotNull(asset.Name);
        Assert.NotEqual(Guid.Empty, asset.AssetId);
        Assert.True(asset.IsTradable | asset.Shortable | asset.EasyToBorrow | asset.Fractionable | asset.Marginable);
    }

    [Fact]
    public async void GetClockWorks()
    {
        var clock = await _alpacaTradingClient.GetClockAsync();

        Assert.NotNull(clock);
        Assert.True(clock.NextOpenUtc > clock.TimestampUtc);
        Assert.True(clock.NextCloseUtc > clock.TimestampUtc);
    }

    [Fact]
    public async void ListCalendarWorks()
    {
        var calendars = await _alpacaTradingClient.ListCalendarAsync(
            new CalendarRequest().WithInterval(
                new Interval<DateOnly>(
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-14)),
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)))));

        Assert.NotNull(calendars);
        Assert.NotEmpty(calendars);

        var first = calendars[0];
        var last = calendars[^1];

        Assert.True(first.TradingDate <= last.TradingDate);
        Assert.True(first.TradingDateUtc <= last.TradingDateUtc);
        Assert.True(first.TradingOpenTimeUtc < first.TradingCloseTimeUtc);
        Assert.True(last.TradingOpenTimeUtc < last.TradingCloseTimeUtc);

        Assert.True(first.OpenTimeEst < first.CloseTimeEst);
        Assert.True(last.OpenTimeUtc < last.CloseTimeUtc);
    }

    [Fact(Skip = "Run too long and sometimes fail")]
    public void AlpacaRestApiThrottlingWorks()
    {
        var tasks = new Task[300];
        for (var i = 0; i < tasks.Length; ++i)
        {
            tasks[i] = _alpacaTradingClient.GetClockAsync();
        }

        Task.WaitAll(tasks);
        Assert.DoesNotContain(tasks, task => task.IsFaulted);
    }
        
    [Fact]
    public async void ListOrdersForDatesWorks()
    {
        var orders = await _alpacaTradingClient.ListOrdersAsync(
            new ListOrdersRequest().WithInterval(
                DateTime.Today.AddDays(-5).GetIntervalTillThat()));

        Assert.NotNull(orders);
        // Assert.NotEmpty(orders);
    }

    public void Dispose() => _alpacaTradingClient?.Dispose();
}
