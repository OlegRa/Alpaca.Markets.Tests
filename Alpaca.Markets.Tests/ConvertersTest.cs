using Newtonsoft.Json;

namespace Alpaca.Markets.Tests;

public sealed class ConvertersTest
{
    private struct Wrapper<T>
    {
        public T Item { get; set; }
    }

    [Fact]
    public void OrderSideEnumConverterWorks()
    {
        using var reader = new JsonTextReader(new StringReader(@"{""item"": ""SellShort""}"));
        var wrapper = new JsonSerializer().Deserialize<Wrapper<OrderSide>>(reader);
        Assert.Equal(OrderSide.Sell, wrapper.Item);
    }
}