namespace Alpaca.Markets.Tests;

[Collection("PaperEnvironment")]
[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public sealed partial class AlpacaCryptoStreamingClientTest : IDisposable
{
    private const String Symbol = "BTCUSD";

    private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

    private readonly IAlpacaTradingClient _alpacaTradingClient;

    public AlpacaCryptoStreamingClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
    {
        _clientsFactory = clientsFactory;
        _alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();
    }

    [SkippableFact]
    public async Task TradesSubscriptionWorks()
    {
        Skip.IfNot(await isCurrentSessionOpenAsync(), "Trading session is closed now.");

        using var client = _clientsFactory.GetAlpacaCryptoStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetTradeSubscription(Symbol);
        subscription.Received += trade =>
        {
            Assert.Equal(Symbol, trade.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromSeconds(30)));

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    [SkippableFact]
    public async Task QuotesSubscriptionWorks()
    {
        Skip.IfNot(await isCurrentSessionOpenAsync(), "Trading session is closed now.");

        using var client = _clientsFactory.GetAlpacaCryptoStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetQuoteSubscription(Symbol);
        subscription.Received += quote =>
        {
            Assert.Equal(Symbol, quote.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromSeconds(10)));

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    [SkippableFact]
    public async Task MinuteBarSubscriptionWorks()
    {
        Skip.IfNot(await isCurrentSessionOpenAsync(), "Trading session is closed now.");

        using var client = _clientsFactory.GetAlpacaCryptoStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetMinuteBarSubscription(Symbol);
        subscription.Received += bar =>
        {
            Assert.Equal(Symbol, bar.Symbol);
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromMinutes(2)));

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    [SkippableFact]
    public async Task AllMinuteBarSubscriptionWorks()
    {
        Skip.IfNot(await isCurrentSessionOpenAsync(), "Trading session is closed now.");

        using var client = _clientsFactory.GetAlpacaCryptoStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObject = new AutoResetEvent(false);

        var subscription = client.GetMinuteBarSubscription();
        subscription.Received += _ =>
        {
            waitObject.Set();
        };

        await client.SubscribeAsync(subscription);

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromMinutes(2)));

        await client.UnsubscribeAsync(subscription);

        await client.DisconnectAsync();
    }

    public void Dispose() => _alpacaTradingClient.Dispose();

    private async Task<Boolean> isCurrentSessionOpenAsync() => 
        (await _alpacaTradingClient.GetClockAsync()).IsOpen;
}
