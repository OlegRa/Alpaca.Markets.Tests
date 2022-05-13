using System;
using System.Threading;
using Xunit;

namespace Alpaca.Markets.Tests;

public sealed partial class AlpacaTradingClientTest
{
    [Fact(Skip = "Not always work correctly")]
    public async void OrderPlaceCheckCancelWorks()
    {
        using var sockClient = _clientsFactory.GetAlpacaStreamingClient();

        static void HandleOnError(Exception ex)
        {
            Assert.Null(ex.Message);
        }

        sockClient.OnError += HandleOnError;

        await sockClient.ConnectAsync();

        var waitObject = new AutoResetEvent(false);

        void HandleOnTradeUpdate(ITradeUpdate update)
        {
            Assert.NotNull(update);
            Assert.NotNull(update.Order);
            Assert.Equal(Symbol, update.Order.Symbol);
            waitObject.Set();
        }

        sockClient.OnTradeUpdate += HandleOnTradeUpdate;

        var clientOrderId = Guid.NewGuid().ToString("N");

        var clock = await _alpacaTradingClient.GetClockAsync();

        var order = await _alpacaTradingClient.PostOrderAsync(
            new NewOrderRequest(
                Symbol, 1, OrderSide.Buy, OrderType.Market,
                clock.IsOpen ? TimeInForce.Day : TimeInForce.Opg)
            {
                ClientOrderId = clientOrderId
            });

        Assert.NotNull(order);
        Assert.Equal(Symbol, order.Symbol);
        Assert.Equal(clientOrderId, order.ClientOrderId);

        var orderById = await _alpacaTradingClient.GetOrderAsync(order.OrderId);
        var orderByClientId = await _alpacaTradingClient.GetOrderAsync(clientOrderId);

        Assert.NotNull(orderById);
        Assert.NotNull(orderByClientId);

        var result = await _alpacaTradingClient.CancelOrderAsync(order.OrderId);

        Assert.True(result);

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromSeconds(10)));

        sockClient.OnTradeUpdate -= HandleOnTradeUpdate;
        sockClient.OnError -= HandleOnError;

        await sockClient.DisconnectAsync();
    }
}
