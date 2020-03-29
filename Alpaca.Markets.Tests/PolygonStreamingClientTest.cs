using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class PolygonStreamingClientTest : IDisposable
    {
        private const String SYMBOL = "AAPL";

        private readonly AlpacaTradingClient _alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

        [Fact]
        public async Task TradesSubscriptionWorks()
        {
            using var client = ClientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);

            client.TradeReceived += (trade) =>
            {
                Assert.Equal(SYMBOL, trade.Symbol);
                waitObject.Set();
            };

            waitObject.Reset();
            client.SubscribeTrade(SYMBOL);

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
            using var client = ClientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);
            client.QuoteReceived += (quote) =>
            {
                Assert.Equal(SYMBOL, quote.Symbol);
                waitObject.Set();
            };

            client.SubscribeQuote(SYMBOL);

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
            using var client = ClientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);
            client.SecondAggReceived += (agg) =>
            {
                Assert.Equal(SYMBOL, agg.Symbol);
                waitObject.Set();
            };

            client.SubscribeSecondAgg(SYMBOL);

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
            using var client = ClientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObject = new AutoResetEvent(false);
            client.MinuteAggReceived += (agg) =>
            {
                Assert.Equal(SYMBOL, agg.Symbol);
                waitObject.Set();
            };

            client.SubscribeMinuteAgg(SYMBOL);

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
            using var client = ClientsFactory.GetPolygonStreamingClient();

            await client.ConnectAndAuthenticateAsync();

            var waitObjects = new []
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };

            client.TradeReceived += (trade) =>
            {
                Assert.Equal(SYMBOL, trade.Symbol);
                waitObjects[0].Set();
            };

            client.QuoteReceived += (quote) =>
            {
                Assert.Equal(SYMBOL, quote.Symbol);
                waitObjects[1].Set();
            };

            client.SubscribeTrade(SYMBOL);
            client.SubscribeQuote(SYMBOL);

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
