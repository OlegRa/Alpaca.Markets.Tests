using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    [Collection("LiveEnvironment")]
    public sealed class PolygonStreamingClientTest : IDisposable
    {
        private const String Symbol = "AAPL";

        private readonly LiveEnvironmentClientsFactoryFixture _clientsFactory;

        private readonly IAlpacaTradingClient _alpacaTradingClient;

        public PolygonStreamingClientTest(LiveEnvironmentClientsFactoryFixture clientsFactory)
        {
            _clientsFactory = clientsFactory;
            _alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();
        }

        [SkippableFact]
        public async Task TradesSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            var subscription = client.GetTradeSubscription(Symbol);
            subscription.Received += (trade) =>
            {
                Assert.Equal(Symbol, trade.Symbol);
                waitObject.Set();
            };
            client.Subscribe(subscription);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        [SkippableFact]
        public async Task QuotesSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            var subscription = client.GetQuoteSubscription(Symbol);
            subscription.Received += (quote) =>
            {
                Assert.Equal(Symbol, quote.Symbol);
                waitObject.Set();
            };
            client.Subscribe(subscription);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        [SkippableFact]
        public async Task SecondAggSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            var subscription = client.GetSecondAggSubscription(Symbol);
            subscription.Received += (agg) =>
            {
                Assert.Equal(Symbol, agg.Symbol);
                waitObject.Set();
            };
            client.Subscribe(subscription);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        [SkippableFact]
        public async Task MinuteAggSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            var subscription = client.GetMinuteAggSubscription(Symbol);
            subscription.Received += (agg) =>
            {
                Assert.Equal(Symbol, agg.Symbol);
                waitObject.Set();
            };
            client.Subscribe(subscription);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(120)));
            }

            await client.DisconnectAsync();
        }

        [SkippableFact]
        public async Task SeveralSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObjects = new []
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };

            var tradeSubscription = client.GetTradeSubscription(Symbol);
            tradeSubscription.Received += (trade) =>
            {
                Assert.Equal(Symbol, trade.Symbol);
                waitObjects[0].Set();
            };

            var quoteSubscription = client.GetQuoteSubscription(Symbol);
            quoteSubscription.Received += (quote) =>
            {
                Assert.Equal(Symbol, quote.Symbol);
                waitObjects[1].Set();
            };

            client.Subscribe(tradeSubscription, quoteSubscription);

            if (await isCurrentSessionOpenAsync())
            {
                // ReSharper disable once CoVariantArrayConversion
                Assert.True(WaitHandle.WaitAll(
                    waitObjects, TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        public void Dispose() => _alpacaTradingClient?.Dispose();

        private async Task<Boolean> isCurrentSessionOpenAsync() => 
            (await _alpacaTradingClient.GetClockAsync()).IsOpen;
    }
}
