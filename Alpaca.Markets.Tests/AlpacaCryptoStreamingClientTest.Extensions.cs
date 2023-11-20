#if EXTENSIONS

using Alpaca.Markets.Extensions;

namespace Alpaca.Markets.Tests;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public sealed partial class AlpacaCryptoStreamingClientTest
{
    [SkippableFact]
    public async Task SeveralSubscriptionWorks()
    {
        Skip.IfNot(await isCurrentSessionOpenAsync(), "Trading session is closed now.");

        using var client = _clientsFactory.GetAlpacaCryptoStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var waitObjects = new[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };

        var tradeSubscription = client.GetTradeSubscription(Symbol);
        tradeSubscription.Received += trade =>
        {
            Assert.Equal(Symbol, trade.Symbol);
            waitObjects[0].Set();
        };

        var quoteSubscription = client.GetQuoteSubscription(Symbol);
        quoteSubscription.Received += quote =>
        {
            Assert.Equal(Symbol, quote.Symbol);
            waitObjects[1].Set();
        };

        await client.SubscribeAsync(tradeSubscription, quoteSubscription);

        // ReSharper disable once CoVariantArrayConversion
        Assert.True(WaitHandle.WaitAll(
            waitObjects, TimeSpan.FromSeconds(10)));

        await client.UnsubscribeAsync(tradeSubscription, quoteSubscription);

        await client.DisconnectAsync();
    }

    [SkippableFact]
    public async Task AsyncEnumerableWorks()
    {
        Skip.IfNot(await isCurrentSessionOpenAsync(), "Trading session is closed now.");

        using var client = _clientsFactory.GetAlpacaCryptoStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(15));

        var subscription = await client.SubscribeQuoteAsync(Symbol);
        await using (subscription.ConfigureAwait(false))
        {
            var atLeastOneQuoteReceived = false;

            await foreach (var _ in subscription
                               .AsAsyncEnumerable(cancellationTokenSource.Token)
                               .ConfigureAwait(false))
            {
                atLeastOneQuoteReceived = true;
                break;
            }

            Assert.True(atLeastOneQuoteReceived);
        }

        await client.DisconnectAsync(CancellationToken.None);
    }
}

#endif