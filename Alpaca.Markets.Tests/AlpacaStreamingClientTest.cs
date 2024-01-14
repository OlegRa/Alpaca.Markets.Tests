namespace Alpaca.Markets.Tests;

[Collection(nameof(PaperEnvironmentClientsFactoryCollection))]
public sealed class AlpacaStreamingClientTest(
    PaperEnvironmentClientsFactoryFixture clientsFactory)
{
    [Fact]
    public async void ConnectWorks()
    {
        using var client = clientsFactory.GetAlpacaStreamingClient();

        client.OnError += HandleOnError;

        await client.ConnectAsync();

        var waitObject = new AutoResetEvent(false);

        client.Connected += HandleOnConnected;

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromSeconds(10)));

        client.Connected -= HandleOnConnected;
        client.OnError -= HandleOnError;

        await client.DisconnectAsync();
        return;

        void HandleOnConnected(AuthStatus status)
        {
            Assert.Equal(AuthStatus.Authorized, status);
            waitObject.Set();
        }

        static void HandleOnError(Exception ex)
        {
            Assert.Null(ex.Message);
        }
    }
}
