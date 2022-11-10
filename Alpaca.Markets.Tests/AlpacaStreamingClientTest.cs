namespace Alpaca.Markets.Tests;

[Collection(nameof(PaperEnvironmentClientsFactoryCollection))]
public sealed class AlpacaStreamingClientTest
{
    private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

    public AlpacaStreamingClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory) => 
        _clientsFactory = clientsFactory;

    [Fact]
    public async void ConnectWorks()
    {
        using var client = _clientsFactory.GetAlpacaStreamingClient();

        static void HandleOnError(Exception ex)
        {
            Assert.Null(ex.Message);
        }

        client.OnError += HandleOnError;

        await client.ConnectAsync();

        var waitObject = new AutoResetEvent(false);

        void HandleOnConnected(AuthStatus status)
        {
            Assert.Equal(AuthStatus.Authorized, status);
            waitObject.Set();
        }

        client.Connected += HandleOnConnected;

        Assert.True(waitObject.WaitOne(
            TimeSpan.FromSeconds(10)));

        client.Connected -= HandleOnConnected;
        client.OnError -= HandleOnError;

        await client.DisconnectAsync();
    }
}
