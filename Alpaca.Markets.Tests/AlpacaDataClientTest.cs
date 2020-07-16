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

        private readonly AlpacaDataClient _alpacaDataClient;

        public AlpacaDataClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory)
        {
            _clientsFactory = clientsFactory;
            _alpacaDataClient = clientsFactory.GetAlpacaDataClient();
        }

        [Fact]
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

        [Fact]
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

            Assert.True(barsList.First().TimeUtc >= dateFrom);
            Assert.True(barsList.Last().TimeUtc <= dateInto);
        }

        [Fact]
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

        
        [Fact]
        public async void GetBarParticularDay()
        {
            var barSet = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest("TSLA", TimeFrame.Minute)
                    .SetInclusiveTimeInterval(
                        new DateTime(2020, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2020, 5, 30, 0, 0, 0, DateTimeKind.Utc)));

            Assert.NotNull(barSet);

            var bars = barSet["TSLA"]
                .OrderBy(_ => _.TimeUtc)
                .ToList();
            Assert.NotNull(bars);
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
