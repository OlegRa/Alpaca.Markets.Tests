using System;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class RestClientExtendedTest : IDisposable
    {
        private const String SYMBOL = "AAPL";

        private readonly AlpacaTradingClient _alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

        private readonly PolygonDataClient _polygonDataClient = ClientsFactory.GetPolygonDataClient();

        [Fact]
        public async void ListHistoricalQuotesReturnsEmptyListForSunday()
        {
            var historicalItems = await _polygonDataClient
                .ListHistoricalQuotesAsync(new HistoricalRequest(
                    SYMBOL, new DateTime(2018, 8, 5)));

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
                    SYMBOL, new AggregationPeriod(1, AggregationPeriodUnit.Day))
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
                    SYMBOL, new AggregationPeriod(1, AggregationPeriodUnit.Minute))
                    {
                        Unadjusted = true
                    }
                    .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(historicalItems);

            Assert.NotNull(historicalItems.Items);
            Assert.NotEmpty(historicalItems.Items);
        }

        [Fact]
        public void AlpacaRestApiThrottlingWorks()
        {
            var tasks = new Task[300];
            for (var i = 0; i < tasks.Length; ++i)
            {
                tasks[i] = _alpacaTradingClient.GetClockAsync();
            }

            Task.WaitAll(tasks);
            Assert.DoesNotContain(tasks, task => task.IsFaulted);
        }
        
        [Fact]
        public async void ListOrdersForDatesWorks()
        {
            var orders = await _alpacaTradingClient.ListOrdersAsync(
                untilDateTimeExclusive: DateTime.Today.AddDays(-5));

            Assert.NotNull(orders);
            // Assert.NotEmpty(orders);
        }

        public void Dispose()
        {
            _polygonDataClient?.Dispose();
        }
    }
}
