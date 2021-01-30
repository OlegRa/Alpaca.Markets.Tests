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

        [SkippableFact]
        public async Task QuotesSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

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

        [SkippableFact]
        public async Task SecondAggSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

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

        [SkippableFact]
        public async Task MinuteAggSubscriptionWorks()
        {
            Skip.If(_clientsFactory.LiveAlpacaIdDoesNotFound);

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

        private async Task<Boolean> isCurrentSessionOpenAsync() => 
            (await _alpacaTradingClient.GetClockAsync()).IsOpen;
    }
}
