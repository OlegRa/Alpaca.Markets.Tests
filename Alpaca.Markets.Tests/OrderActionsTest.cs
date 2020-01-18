using System;
using System.Threading;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class OrderActionsTest : IDisposable
    {
        private const String SYMBOL = "AAPL";

        private readonly AlpacaTradingClient _alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

        [Fact]
        public async void OrderPlaceCheckCancelWorks()
        {
            using var sockClient = ClientsFactory.GetAlpacaStreamingClient();

            sockClient.OnError += (ex) =>
            {
                Assert.Null(ex.Message);
            };

            await sockClient.ConnectAsync();

            var waitObject = new AutoResetEvent(false);
            sockClient.OnTradeUpdate += (update) =>
            {
                Assert.NotNull(update);
                Assert.NotNull(update.Order);
                Assert.Equal(SYMBOL, update.Order.Symbol);
                waitObject.Set();
            };

            var clientOrderId = Guid.NewGuid().ToString("N");

            var clock = await _alpacaTradingClient.GetClockAsync();

            var order = await _alpacaTradingClient.PostOrderAsync(
                SYMBOL, 1, OrderSide.Buy, OrderType.Market,
                clock.IsOpen ? TimeInForce.Day : TimeInForce.Opg,
                clientOrderId: clientOrderId);

            Assert.NotNull(order);
            Assert.Equal(SYMBOL, order.Symbol);
            Assert.Equal(clientOrderId, order.ClientOrderId);

            var orderById = await _alpacaTradingClient.GetOrderAsync(order.OrderId);
            var orderByClientId = await _alpacaTradingClient.GetOrderAsync(clientOrderId);

            Assert.NotNull(orderById);
            Assert.NotNull(orderByClientId);

            var result = await _alpacaTradingClient.DeleteOrderAsync(order.OrderId);

            Assert.True(result);

            Assert.True(waitObject.WaitOne(
                TimeSpan.FromSeconds(10)));

            await sockClient.DisconnectAsync();
        }

        public void Dispose()
        {
            _alpacaTradingClient?.Dispose();
        }
    }
}
