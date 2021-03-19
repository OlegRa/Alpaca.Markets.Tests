using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
#pragma warning disable 618

namespace Alpaca.Markets.Tests
{
    [Collection("PaperEnvironment")]
    public sealed class AlpacaDataStreamingClientTest : IDisposable
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

            client.Unsubscribe(subscription);

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task QuotesSubscriptionWorks()
        {
            using var client = _clientsFactory.GetAlpacaDataStreamingClient();

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

            client.Unsubscribe(subscription);

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task MinuteAggSubscriptionWorks()
        {
            using var client = _clientsFactory.GetAlpacaDataStreamingClient();

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
                    TimeSpan.FromMinutes(2)));
            }

            client.Unsubscribe(subscription);

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task AllMinuteAggSubscriptionWorks()
        {
            using var client = _clientsFactory.GetAlpacaDataStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            var subscription = client.GetMinuteAggSubscription();
            subscription.Received += (agg) =>
            {
                Assert.Equal(Symbol, agg.Symbol);
                waitObject.Set();
            };

            client.Subscribe(subscription);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromMinutes(2)));
            }

            client.Unsubscribe(subscription);

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task SeveralSubscriptionWorks()
        {
            using var client = _clientsFactory.GetAlpacaDataStreamingClient();

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
            client.Unsubscribe(tradeSubscription, quoteSubscription);

            await client.DisconnectAsync();
        }

        public void Dispose() => _alpacaTradingClient?.Dispose();

        private async Task<Boolean> isCurrentSessionOpenAsync()
        {
            return (await _alpacaTradingClient.GetClockAsync()).IsOpen;
        }
    }
}
