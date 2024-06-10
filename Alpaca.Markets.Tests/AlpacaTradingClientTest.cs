namespace Alpaca.Markets.Tests;

[Collection(nameof(PaperEnvironmentClientsFactoryCollection))]
public sealed partial class AlpacaTradingClientTest(
    PaperEnvironmentClientsFactoryFixture clientsFactory) : IDisposable
{
    private const String Symbol = "AAPL";

    private readonly IAlpacaTradingClient _alpacaTradingClient = clientsFactory.GetAlpacaTradingClient();

    [Fact]
    public async Task GetPortfolioHistoryAsyncWorks()
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
    public async Task GetAssetAsyncThrowsCustomException()
    {
        var exception = await Assert.ThrowsAsync<RestClientErrorException>(
            () => _alpacaTradingClient.GetAssetAsync("HEI-A"));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.NotEqual(0, exception.ErrorCode);
    }

    [Fact]
    public async Task GetAccountWorks()
    {
        var account = await _alpacaTradingClient.GetAccountAsync();

        Assert.NotNull(account);
        Assert.Equal("USD", account.Currency);
    }

    [Fact]
    public async Task GetAccountConfigurationWorks()
    {
        var accountConfiguration = await _alpacaTradingClient.GetAccountConfigurationAsync();

        Assert.NotNull(accountConfiguration);
        Assert.False(accountConfiguration.IsNoShorting);
    }

    [Fact]
    public async Task PatchAccountConfigurationWorks()
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
    public async Task ListOrdersWorks()
    {
        var orders = await _alpacaTradingClient.ListOrdersAsync(new ListOrdersRequest());

        Assert.NotNull(orders);
        // Assert.NotEmpty(orders);
    }

    [Fact(Skip = "Temporary disabled due to problems with order requesting.")]
    public async Task GetOrderWorks()
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
    public async Task ListPositionsWorks()
    {
        var positions = await _alpacaTradingClient.ListPositionsAsync();

        Assert.NotNull(positions);
        Assert.NotEmpty(positions);
    }

    [Fact]
    public async Task GetPositionWorks()
    {
        var position = await _alpacaTradingClient.GetPositionAsync(Symbol);

        Assert.NotNull(position);
        Assert.Equal(Symbol, position.Symbol);
    }

    [Fact]
    public async Task ListAssetsWorks()
    {
        var assets = await _alpacaTradingClient.ListAssetsAsync(
            new AssetsRequest { AssetClass = AssetClass.Crypto, AssetStatus = AssetStatus.Active});

        Assert.NotNull(assets);
        Assert.NotEmpty(assets);
    }

    [Fact]
    public async Task GetAssetWorks()
    {
        var asset = await _alpacaTradingClient.GetAssetAsync(Symbol);

        Assert.NotNull(asset);
        Assert.Equal(Symbol, asset.Symbol);

        Assert.NotNull(asset.Name);
        Assert.NotEqual(Guid.Empty, asset.AssetId);
        Assert.True(asset.IsTradable | asset.Shortable | asset.EasyToBorrow | asset.Fractionable | asset.Marginable);
    }

    [Fact]
    public async Task GetClockWorks()
    {
        var clock = await _alpacaTradingClient.GetClockAsync();

        Assert.NotNull(clock);
        Assert.True(clock.NextOpenUtc > clock.TimestampUtc);
        Assert.True(clock.NextCloseUtc > clock.TimestampUtc);
    }

    [Fact]
    public async Task ListCalendarWorks()
    {
        var calendars = await _alpacaTradingClient.ListIntervalCalendarAsync(
            new CalendarRequest().WithInterval(
                new Interval<DateOnly>(
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-14)),
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)))));

        Assert.NotNull(calendars);
        Assert.NotEmpty(calendars);

        var first = calendars[0].Trading;
        var last = calendars[^1].Trading;

        Assert.True(first.OpenEst.Date <= last.OpenEst.Date);
        Assert.True(first.OpenEst < first.CloseEst);
        Assert.True(last.OpenEst < last.CloseEst);

        first = calendars[0].Session;
        last = calendars[^1].Session;

        Assert.True(first.OpenEst.Date <= last.OpenEst.Date);
        Assert.True(first.OpenEst < first.CloseEst);
        Assert.True(last.OpenEst < last.CloseEst);
    }

    [Fact(Skip = "Run too long and sometimes fail")]
    public async Task AlpacaRestApiThrottlingWorks()
    {
        var tasks = new Task[300];
        for (var i = 0; i < tasks.Length; ++i)
        {
            tasks[i] = _alpacaTradingClient.GetClockAsync();
        }

        await Task.WhenAll(tasks);
        Assert.DoesNotContain(tasks, task => task.IsFaulted);
    }
        
    [Fact]
    public async Task ListOrdersForDatesWorks()
    {
        var orders = await _alpacaTradingClient.ListOrdersAsync(
            new ListOrdersRequest().WithInterval(
                DateTime.Today.AddDays(-5).GetIntervalTillThat()));

        Assert.NotNull(orders);
        // Assert.NotEmpty(orders);
    }

    public void Dispose() => _alpacaTradingClient.Dispose();
}
