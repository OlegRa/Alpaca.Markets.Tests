﻿#if EXTENSIONS

using Alpaca.Markets.Extensions;

namespace Alpaca.Markets.Tests;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public sealed partial class AlpacaDataStreamingClientTest
{
    [Fact]
    public async Task SeveralSubscriptionWorks()
    {
        using var client = _clientsFactory.GetAlpacaDataStreamingClient();

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

        if (await isCurrentSessionOpenAsync())
        {
            // ReSharper disable once CoVariantArrayConversion
            Assert.True(WaitHandle.WaitAll(
                waitObjects, TimeSpan.FromSeconds(10)));
        }
        await client.UnsubscribeAsync(tradeSubscription, quoteSubscription);

        await client.DisconnectAsync();
    }

    [Fact]
    public async Task AsyncEnumerableWorks()
    {
        using var client = _clientsFactory.GetAlpacaDataStreamingClient();

        await client.ConnectAndAuthenticateAsync();

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(15));

        var subscription = await client.SubscribeTradeAsync(Symbol);
        await using (subscription.ConfigureAwait(false))
        {
            if (await isCurrentSessionOpenAsync())
            {
                var atLeastOneTradeReceived = false;

                await foreach (var _ in subscription
                                   .AsAsyncEnumerable(cancellationTokenSource.Token)
                                   .ConfigureAwait(false))
                {
                    atLeastOneTradeReceived = true;
                    break;
                }

                Assert.True(atLeastOneTradeReceived);
            }
        }

        await client.DisconnectAsync(CancellationToken.None);
    }
}

#endif