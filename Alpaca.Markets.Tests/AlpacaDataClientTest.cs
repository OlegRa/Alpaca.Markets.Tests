using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class AlpacaDataClientTest : IDisposable
    {
        private const String Symbol = "AAPL";

        private readonly AlpacaDataClient _alpacaDataClient = ClientsFactory.GetAlpacaDataClient();

        [Fact(Skip = "Valid keys required")]
        public async void GetBarSetWorks()
        {
            var barSet = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(Symbol, TimeFrame.Day));

            Assert.NotNull(barSet);
            Assert.Equal(1, barSet.Count);

            Assert.True(barSet.ContainsKey(Symbol));

            var bars = barSet[Symbol];
            Assert.NotNull(bars);
            Assert.NotEmpty(bars);
        }

        [Fact(Skip = "Valid keys required")]
        public async void GetBarSetForTimeScopeWorks()
        {
            var dateInto = await getLastTradingDay();
            var dateFrom = dateInto.AddHours(-20);

            var barSet = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(Symbol, TimeFrame.FifteenMinutes)
                    .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(barSet);
            Assert.Equal(1, barSet.Count);

            Assert.True(barSet.ContainsKey(Symbol));

            var bars = barSet[Symbol];
            Assert.NotNull(bars);

            var barsList = bars.ToList();
            Assert.NotEmpty(barsList);
            Assert.True(barsList.Count >= 2);

            Assert.True(barsList.First().Time >= dateFrom);
            Assert.True(barsList.Last().Time <= dateInto);
        }

        [Fact(Skip = "Valid keys required")]
        public async void GetBarSetForTwoSymbolsWorks()
        {
            const Int32 limit = 10;
            var symbols = new[] { Symbol, "MSFT" };
            var barSet = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(symbols, TimeFrame.Minute)
                {
                    Limit = limit
                });

            Assert.NotNull(barSet);
            Assert.Equal(symbols.Length, barSet.Count);

            foreach (var symbol in symbols)
            {
                Assert.True(barSet.ContainsKey(symbol));

                var bars = barSet[Symbol];
                Assert.NotNull(bars);

                var barsList = bars.ToList();
                Assert.NotEmpty(barsList);
                Assert.Equal(limit, barsList.Count);
            }
        }

        private async Task<DateTime> getLastTradingDay()
        {
            using var alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

            var calendars = await alpacaTradingClient
                .ListCalendarAsync(new CalendarRequest()
                    .SetInclusiveTimeInterval(
                        DateTime.UtcNow.Date.AddDays(-14),
                        DateTime.UtcNow.Date.AddDays(-1)));

            Assert.NotNull(calendars);

            return calendars.Last().TradingCloseTime;
        }

        public void Dispose() => _alpacaDataClient?.Dispose();
    }
}
