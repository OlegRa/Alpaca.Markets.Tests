using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    [Collection("Alpaca.Markets.Tests")]
    public abstract class PolygonStreamingClientTest : IDisposable
    {
        private const String Symbol = "AAPL";

        private readonly ClientsFactoryFixture _clientsFactory;

        private readonly AlpacaTradingClient _alpacaTradingClient;

        public PolygonStreamingClientTest(ClientsFactoryFixture clientsFactory)
        {
            _clientsFactory = clientsFactory;
            _alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();
        }

        [Fact]
        public async Task TradesSubscriptionWorks()
        {
            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            client.TradeReceived += (trade) =>
            {
                Assert.Equal(Symbol, trade.Symbol);
                waitObject.Set();
            };

            waitObject.Reset();
            client.SubscribeTrade(Symbol);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task QuotesSubscriptionWorks()
        {
            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);
            client.QuoteReceived += (quote) =>
            {
                Assert.Equal(Symbol, quote.Symbol);
                waitObject.Set();
            };

            client.SubscribeQuote(Symbol);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task SecondAggSubscriptionWorks()
        {
            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);
            client.SecondAggReceived += (agg) =>
            {
                Assert.Equal(Symbol, agg.Symbol);
                waitObject.Set();
            };

            client.SubscribeSecondAgg(Symbol);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task MinuteAggSubscriptionWorks()
        {
            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);
            client.MinuteAggReceived += (agg) =>
            {
                Assert.Equal(Symbol, agg.Symbol);
                waitObject.Set();
            };

            client.SubscribeMinuteAgg(Symbol);

            if (await isCurrentSessionOpenAsync())
            {
                Assert.True(waitObject.WaitOne(
                    TimeSpan.FromSeconds(120)));
            }

            await client.DisconnectAsync();
        }

        [Fact]
        public async Task SeveralSubscriptionWorks()
        {
            using var client = _clientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObjects = new []
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };

            client.TradeReceived += (trade) =>
            {
                Assert.Equal(Symbol, trade.Symbol);
                waitObjects[0].Set();
            };

            client.QuoteReceived += (quote) =>
            {
                Assert.Equal(Symbol, quote.Symbol);
                waitObjects[1].Set();
            };

            client.SubscribeTrade(Symbol);
            client.SubscribeQuote(Symbol);

            if (await isCurrentSessionOpenAsync())
            {
                // ReSharper disable once CoVariantArrayConversion
                Assert.True(WaitHandle.WaitAll(
                    waitObjects, TimeSpan.FromSeconds(10)));
            }

            await client.DisconnectAsync();
        }

        public void Dispose() => _alpacaTradingClient?.Dispose();

        private async Task<Boolean> isCurrentSessionOpenAsync()
        {
            return (await _alpacaTradingClient.GetClockAsync()).IsOpen;
        }
    }
}
