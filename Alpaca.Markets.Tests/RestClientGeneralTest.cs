using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class RestClientGeneralTest : IDisposable
    {
        private const String SYMBOL = "AAPL";

        private readonly AlpacaTradingClient _alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

        private readonly AlpacaDataClient _alpacaDataClient = ClientsFactory.GetAlpacaDataClient();

        [Fact]
        public async void GetAccountWorks()
        {
            var account = await _alpacaTradingClient.GetAccountAsync();

            Assert.NotNull(account);
            Assert.Equal("USD", account.Currency);
        }

        [Fact]
        public async void GetAccountConfigurationWorks()
        {
            var accountConfiguration = await _alpacaTradingClient.GetAccountConfigurationAsync();

            Assert.NotNull(accountConfiguration);
            Assert.False(accountConfiguration.IsNoShorting);
        }

        [Fact]
        public async void PatchAccountConfigurationWorks()
        {
            var accountConfigurationOld = await _alpacaTradingClient.GetAccountConfigurationAsync();

            Assert.NotNull(accountConfigurationOld);

            accountConfigurationOld.TradeConfirmEmail =
                accountConfigurationOld.TradeConfirmEmail == TradeConfirmEmail.All
                    ? TradeConfirmEmail.None
                    : TradeConfirmEmail.All;

            var accountConfigurationNew = await _alpacaTradingClient.PatchAccountConfigurationAsync(accountConfigurationOld);

            Assert.NotNull(accountConfigurationNew);
            Assert.NotEqual(accountConfigurationNew, accountConfigurationOld);

            Assert.Equal(accountConfigurationOld.TradeConfirmEmail, accountConfigurationNew.TradeConfirmEmail);
        }

        [Fact]
        public async void ListOrdersWorks()
        {
            var orders = await _alpacaTradingClient.ListOrdersAsync(new ListOrdersRequest());

            Assert.NotNull(orders);
            // Assert.NotEmpty(orders);
        }

        [Fact]
        public async void GetOrderWorks()
        {
            var orders = await _alpacaTradingClient.ListOrdersAsync(
                new ListOrdersRequest { OrderStatusFilter = OrderStatusFilter.All });

            Assert.NotNull(orders);

            var ordersList = orders.ToList();
            Assert.NotEmpty(ordersList);
            var first = ordersList.First();

            var orderById = await _alpacaTradingClient.GetOrderAsync(first.OrderId);
            var orderByClientId = await _alpacaTradingClient.GetOrderAsync(first.ClientOrderId);

            Assert.NotNull(orderById);
            Assert.NotNull(orderByClientId);

            Assert.Equal(orderById.OrderId, orderByClientId.OrderId);
            Assert.Equal(orderById.ClientOrderId, orderByClientId.ClientOrderId);
        }

        [Fact]
        public async void ListPositionsWorks()
        {
            var positions = await _alpacaTradingClient.ListPositionsAsync();

            Assert.NotNull(positions);
            Assert.NotEmpty(positions);
        }

        [Fact]
        public async void GetPositionWorks()
        {
            var position = await _alpacaTradingClient.GetPositionAsync(SYMBOL);

            Assert.NotNull(position);
            Assert.Equal(SYMBOL, position.Symbol);
        }

        [Fact]
        public async void ListAssetsWorks()
        {
            var assets = await _alpacaTradingClient.ListAssetsAsync(
                new AssetsRequest());

            Assert.NotNull(assets);
            Assert.NotEmpty(assets);
        }

        [Fact]
        public async void GetAssetWorks()
        {
            var asset = await _alpacaTradingClient.GetAssetAsync(SYMBOL);

            Assert.NotNull(asset);
            Assert.Equal(SYMBOL, asset.Symbol);
        }

        [Fact]
        public async void GetClockWorks()
        {
            var clock = await _alpacaTradingClient.GetClockAsync();

            Assert.NotNull(clock);
            Assert.True(clock.NextOpen > clock.Timestamp);
            Assert.True(clock.NextClose > clock.Timestamp);
        }

        [Fact]
        public async void ListCalendarWorks()
        {
            var calendars = await _alpacaTradingClient.ListCalendarAsync(
                new CalendarRequest().SetInclusiveTimeInterval(
                    DateTime.Today.AddDays(-14),
                    DateTime.Today.AddDays(14)));

            Assert.NotNull(calendars);

            var calendarsList = calendars.ToList();

            Assert.NotEmpty(calendarsList);

            var first = calendarsList.First();
            var last = calendarsList.Last();

            Assert.True(first.TradingDate <= last.TradingDate);
            Assert.True(first.TradingOpenTime < first.TradingCloseTime);
            Assert.True(last.TradingOpenTime < last.TradingCloseTime);
        }

        [Fact]
        public async void GetBarSetWorks()
        {
            var barSet = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(SYMBOL, TimeFrame.Day));

            Assert.NotNull(barSet);
            Assert.Equal(1, barSet.Count);

            Assert.True(barSet.ContainsKey(SYMBOL));

            var bars = barSet[SYMBOL];
            Assert.NotNull(bars);
            Assert.NotEmpty(bars);
        }

        [Fact]
        public async void GetBarSetForTimeScopeWorks()
        {
            var dateInto = await getLastTradingDay();
            var dateFrom = dateInto.AddHours(-20);

            var barSet = await _alpacaDataClient.GetBarSetAsync(
                new BarSetRequest(SYMBOL, TimeFrame.FifteenMinutes)
                    .SetInclusiveTimeInterval(dateFrom, dateInto));

            Assert.NotNull(barSet);
            Assert.Equal(1, barSet.Count);

            Assert.True(barSet.ContainsKey(SYMBOL));

            var bars = barSet[SYMBOL];
            Assert.NotNull(bars);

            var barsList = bars.ToList();
            Assert.NotEmpty(barsList);
            Assert.True(barsList.Count >= 2);

            Assert.True(barsList.First().Time >= dateFrom);
            Assert.True(barsList.Last().Time <= dateInto);
        }

        [Fact]
        public async void GetBarSetForTwoSymbolsWorks()
        {
            const Int32 limit = 10;
            var symbols = new[] { SYMBOL, "MSFT" };
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

                var bars = barSet[SYMBOL];
                Assert.NotNull(bars);

                var barsList = bars.ToList();
                Assert.NotEmpty(barsList);
                Assert.Equal(limit, barsList.Count);
            }
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
            _alpacaTradingClient?.Dispose();
        }
    }
}
