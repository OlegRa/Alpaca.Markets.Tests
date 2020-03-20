using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class RestClientPolygonTest : IDisposable
    {
        private const String SYMBOL = "AAPL";

        private readonly AlpacaTradingClient _alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

        private readonly PolygonDataClient _polygonDataClient = ClientsFactory.GetPolygonDataClient();

        [Fact]
        public async void ListExchangesWorks()
        {
            var exchanges = await _polygonDataClient.ListExchangesAsync();

            Assert.NotNull(exchanges);
            Assert.NotEmpty(exchanges);
        }

        [Fact]
        public async void GetSymbolTypeMapWorks()
        {
            var symbolTypeMap = await _polygonDataClient.GetSymbolTypeMapAsync();

            Assert.NotNull(symbolTypeMap);
            Assert.NotEmpty(symbolTypeMap);
        }

        [Fact]
        public async void ListHistoricalTradesWorks()
        {
            var historicalItems = await _polygonDataClient
                .ListHistoricalTradesAsync(
                    new HistoricalRequest(
                        SYMBOL, await getLastTradingDay()));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact]
        public async void ListHistoricalQuotesWorks()
        {
            var historicalItems = await _polygonDataClient
                .ListHistoricalQuotesAsync(
                    new HistoricalRequest(
                        SYMBOL, await getLastTradingDay()));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact]
        public async void ListMinuteAggregatesForDateRangeWorks()
        {
            var dateInto = await getLastTradingDay();
            var dateFrom = dateInto.AddHours(-20);

            var historicalItems = await _polygonDataClient
                .ListAggregatesAsync(
                    new AggregatesRequest(
                        SYMBOL, new AggregationPeriod(1, AggregationPeriodUnit.Minute))
                        .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact]
        public async void GetLastTradeWorks()
        {
            var lastTrade = await _polygonDataClient
                .GetLastTradeAsync(SYMBOL);

            Assert.NotNull(lastTrade);
            Assert.True(lastTrade.Time.Kind == DateTimeKind.Utc);
        }

        [Fact]
        public async void GetLastQuoteWorks()
        {
            var lastQuote = await _polygonDataClient
                .GetLastQuoteAsync(SYMBOL);

            Assert.NotNull(lastQuote);
            Assert.True(lastQuote.Time.Kind == DateTimeKind.Utc);
        }

        [Theory]
        [InlineData(TickType.Trades)]
        [InlineData(TickType.Quotes)]
        public async void GetConditionMapForQuotesWorks(
            TickType tickType)
        {
            var conditionMap = await _polygonDataClient
                .GetConditionMapAsync(tickType);

            Assert.NotNull(conditionMap);
            Assert.NotEmpty(conditionMap);
        }

        private async Task<DateTime> getLastTradingDay()
        {
            var calendars = await _alpacaTradingClient
                .ListCalendarAsync(new CalendarRequest()
                    .SetInclusiveTimeInterval(
                        DateTime.UtcNow.Date.AddDays(-14),
                        DateTime.UtcNow.Date.AddDays(-1)));

            Assert.NotNull(calendars);

            return calendars.Last().TradingCloseTime;
        }

        public void Dispose()
        {
            _polygonDataClient?.Dispose();
        }
    }
}