using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alpaca.Markets.Extensions;
using Xunit;

namespace Alpaca.Markets.Tests
{
    [Collection("PaperEnvironment")]
    public sealed class AlpacaDataClientTest : IDisposable
    {
        private const String Symbol = "AAPL";

        private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

        private readonly IAlpacaDataClient _alpacaDataClient;

        public AlpacaDataClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
        {
            _clientsFactory = clientsFactory;
            _alpacaDataClient = clientsFactory.GetAlpacaDataClient();
        }

        [Fact(Skip="Doesn't work right now - server side issues.")]
        public async void ListHistoricalBarsWorks()
        {
            var into = (await getLastTradingDay()).Date;
            var from = into.AddDays(-5).Date;
            var bars = await _alpacaDataClient.ListHistoricalBarsAsync(
                new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Hour));

            Assert.NotNull(bars);
            Assert.NotNull(bars.Items);
            Assert.NotEmpty(bars.Items);
        }

        [Fact]
        public async void GetHistoricalBarsAsAsyncEnumerableWorks()
        {
            var into = (await getLastTradingDay()).Date;
            var from = into.AddDays(-5).Date;
            await foreach (var bar in _alpacaDataClient
                .GetHistoricalBarsAsAsyncEnumerable(
                    new HistoricalBarsRequest(Symbol, from, into, BarTimeFrame.Hour)))
            {
                Assert.NotNull(bar);
                Assert.NotNull(bar.TimeUtc);
                Assert.InRange(bar.TimeUtc.Value, from, into);
            }
        }

        [Fact]
        public async void ListHistoricalQuotesWorks()
        {
            var into = (await getLastTradingDay()).Date;
            var from = into.AddDays(-3).Date;
            var quotes = await _alpacaDataClient.ListHistoricalQuotesAsync(
                new HistoricalQuotesRequest(Symbol, from, into));

            Assert.NotNull(quotes);
            Assert.NotNull(quotes.Items);
            Assert.NotEmpty(quotes.Items);
        }


        [Fact]
        public async void GetHistoricalQuotesAsAsyncEnumerableWorks()
        {
            var into = (await getLastTradingDay()).Date;
            var from = into.AddDays(-3).Date;

            var count = 0;
            var cancellationTokenSource = new CancellationTokenSource(
                TimeSpan.FromSeconds(30));
            try
            {
                await foreach (var quote in _alpacaDataClient
                    .GetHistoricalQuotesAsAsyncEnumerable(
                        new HistoricalQuotesRequest(Symbol, from, into))
                    .WithCancellation(cancellationTokenSource.Token))
                {
                    Assert.NotNull(quote);
                    Assert.InRange(quote.TimestampUtc, from, into);
                    ++count;
                }
            }
            catch (OperationCanceledException)
            {
                Assert.True(cancellationTokenSource.IsCancellationRequested);
            }

            Assert.NotEqual(0, count);
        }

        [Fact]
        public async void ListHistoricalTradesWorks()
        {
            var into = (await getLastTradingDay()).Date;
            var from = into.AddDays(-3).Date;
            var quotes = await _alpacaDataClient.ListHistoricalTradesAsync(
                new HistoricalTradesRequest(Symbol, from, into));

            Assert.NotNull(quotes);
            Assert.NotNull(quotes.Items);
            Assert.NotEmpty(quotes.Items);
        }

        [Fact]
        public async void GetLBarSetWorks()
        {
            var dictionary = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(Symbol, TimeFrame.Day) { Limit = 10 });

            Assert.NotNull(dictionary);
            Assert.Contains(Symbol, dictionary);
            Assert.Equal(10, dictionary[Symbol].Count);
        }

        [Fact]
        public async void GetLBarSetForAllSymbolsWorks()
        {
            var assets = await _clientsFactory.GetAlpacaTradingClient()
                .ListAssetsAsync(new AssetsRequest {AssetStatus = AssetStatus.Active});
            var symbols = assets.Select(_ => _.Symbol).Take(100).ToList();
                
            var dictionary = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(symbols, TimeFrame.Day)
                {
                    Limit = 10
                });

            Assert.NotNull(dictionary);
            Assert.Equal(symbols.Count, dictionary.Count);
            Assert.Contains(symbols[0], dictionary);
            Assert.Equal(10, dictionary[symbols[0]].Count);
        }

        [Fact]
        public async void GetLastQuoteWorks()
        {
            var quote = await _alpacaDataClient.GetLastQuoteAsync(Symbol);

            Assert.NotNull(quote);
            Assert.Equal(Symbol, quote.Symbol);
        }

        [Fact]
        public async void GetLastTradeWorks()
        {
            var trade = await _alpacaDataClient.GetLastTradeAsync(Symbol);

            Assert.NotNull(trade);
            Assert.Equal(Symbol, trade.Symbol);
        }

        private async Task<DateTime> getLastTradingDay()
        {
            using var alpacaTradingClient = _clientsFactory.GetAlpacaTradingClient();

            var calendars = await alpacaTradingClient
                .ListCalendarAsync(new CalendarRequest()
                    .SetInclusiveTimeInterval(
                        DateTime.UtcNow.Date.AddDays(-14),
                        DateTime.UtcNow.Date.AddDays(-1)));

            Assert.NotNull(calendars);

            return calendars.Last().TradingCloseTimeUtc;
        }

        public void Dispose() => _alpacaDataClient?.Dispose();
    }
}
