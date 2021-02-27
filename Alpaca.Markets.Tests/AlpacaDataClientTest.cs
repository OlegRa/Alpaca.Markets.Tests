using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async void GetBarsWorks()
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
        public async void GetHistoricalQuotesWorks()
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
        public async void GetHistoricalTradesWorks()
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
