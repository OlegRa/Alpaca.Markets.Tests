namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public sealed partial class AlpacaDataStreamingClientTest : IDisposable
{
    private const String Symbol = "AAPL";

    private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

    private readonly IAlpacaTradingClient _alpacaTradingClient;

    public AlpacaDataStreamingClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
    {
        _clientsFactory = clientsFactory;
        _alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();
    }

    [Fact]
    public async Task TradesSubscriptionWorks()
    {
        using var client = _clientsFactory.GetAlpacaDataStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetTradeSubscription(Symbol);
        subscription.Received += trade =>
        {
            Assert.Equal(Symbol, trade.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        if (await isCurrentSessionOpenAsync())
        {
            Assert.True(waitObject.WaitOne(
                TimeSpan.FromSeconds(10)));
        }

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    [Fact]
    public async Task QuotesSubscriptionWorks()
    {
        using var client = _clientsFactory.GetAlpacaDataStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetQuoteSubscription(Symbol);
        subscription.Received += quote =>
        {
            Assert.Equal(Symbol, quote.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        if (await isCurrentSessionOpenAsync())
        {
            Assert.True(waitObject.WaitOne(
                TimeSpan.FromSeconds(10)));
        }

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    [Fact]
    public async Task MinuteAggSubscriptionWorks()
    {
        using var client = _clientsFactory.GetAlpacaDataStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetMinuteBarSubscription(Symbol);
        subscription.Received += bar =>
        {
            Assert.Equal(Symbol, bar.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        if (await isCurrentSessionOpenAsync())
        {
            Assert.True(waitObject.WaitOne(
                TimeSpan.FromMinutes(2)));
        }

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    [Fact]
    public async Task AllMinuteAggSubscriptionWorks()
    {
        using var client = _clientsFactory.GetAlpacaDataStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetMinuteBarSubscription();
        subscription.Received += bar =>
        {
            Assert.Equal(Symbol, bar.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        if (await isCurrentSessionOpenAsync())
        {
            Assert.True(waitObject.WaitOne(
                TimeSpan.FromMinutes(2)));
        }

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    public void Dispose() => _alpacaTradingClient?.Dispose();

    private async Task<Boolean> isCurrentSessionOpenAsync()
    {
        return (await _alpacaTradingClient.GetClockAsync()).IsOpen;
    }
}
