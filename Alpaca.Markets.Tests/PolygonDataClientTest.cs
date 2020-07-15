using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    [Collection("PaperEnvironment")]
    public abstract class PolygonDataClientTest : IDisposable
    {
        private const String Symbol = "AAPL";

        private readonly LiveEnvironmentClientsFactoryFixture _clientsFactory;

        private readonly PolygonDataClient _polygonDataClient;

        public PolygonDataClientTest(LiveEnvironmentClientsFactoryFixture clientsFactory)
        {
            _clientsFactory = clientsFactory;
            _polygonDataClient = _clientsFactory.GetPolygonDataClient();
        }

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
                        Symbol, await getLastTradingDay()));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact(Skip = "Works fine only on Live environment")]
        public async void ListHistoricalQuotesWorks()
        {
            var historicalItems = await _polygonDataClient
                .ListHistoricalQuotesAsync(
                    new HistoricalRequest(
                        Symbol, await getLastTradingDay()));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact(Skip = "Works fine only on Live environment")]
        public async void ListMinuteAggregatesForDateRangeWorks()
        {
            var dateInto = await getLastTradingDay();
            var dateFrom = dateInto.AddHours(-20);

            var historicalItems = await _polygonDataClient
                .ListAggregatesAsync(
                    new AggregatesRequest(
                        Symbol, new AggregationPeriod(1, AggregationPeriodUnit.Minute))
                        .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact]
        public async void GetLastTradeWorks()
        {
            var lastTrade = await _polygonDataClient
                .GetLastTradeAsync(Symbol);

            Assert.NotNull(lastTrade);
            Assert.True(lastTrade.TimeUtc.Kind == DateTimeKind.Utc);
        }

        [Fact]
        public async void GetLastQuoteWorks()
        {
            var lastQuote = await _polygonDataClient
                .GetLastQuoteAsync(Symbol);

            Assert.NotNull(lastQuote);
            Assert.True(lastQuote.TimeUtc.Kind == DateTimeKind.Utc);
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

        [Fact(Skip = "Not working - returns 404 now")]
        public async void ListHistoricalQuotesReturnsEmptyListForSunday()
        {
            var sunday = DateTime.UtcNow.Date;
            while (sunday.DayOfWeek != DayOfWeek.Sunday)
            {
                sunday = sunday.AddDays(-1);
            }

            var historicalItems = await _polygonDataClient
                .ListHistoricalQuotesAsync(new HistoricalRequest(Symbol, sunday));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.Empty(historicalItems.Items);
        }

        [Fact]
        public async void ListDayAggregatesForSpecificDatesWorks()
        {
            var dateInto = DateTime.Today;
            var dateFrom = dateInto.AddDays(-7);

            var historicalItems = await _polygonDataClient
                .ListAggregatesAsync(new AggregatesRequest(
                        Symbol, new AggregationPeriod(1, AggregationPeriodUnit.Day))
                    .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact]
        public async void ListMinuteAggregatesForSpecificDatesWorks()
        {
            var dateInto = DateTime.Today;
            var dateFrom = dateInto.AddDays(-7);

            var historicalItems = await _polygonDataClient
                .ListAggregatesAsync(new AggregatesRequest(
                        Symbol, new AggregationPeriod(1, AggregationPeriodUnit.Minute))
                    {
                        Unadjusted = true
                    }
                    .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
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

        public void Dispose()
        {
            _polygonDataClient?.Dispose();
        }
    }
}