using System;
using System.Threading;
using Xunit;

namespace Alpaca.Markets.Tests
{
    [Collection("PaperEnvironment")]
    public sealed class AlpacaStreamingClientTest
    {
        private readonly PaperEnvironmentClientsFactoryFixture _clientsFactory;

        public AlpacaStreamingClientTest(PaperEnvironmentClientsFactoryFixture clientsFactory) => 
            _clientsFactory = clientsFactory;

        [Fact]
        public async void ConnectWorks()
        {
            using var client = _clientsFactory.GetAlpacaStreamingClient();

            client.OnError += (ex) =>
            {
                Assert.Null(ex.Message);
            };

            await client.ConnectAsync();

            var waitObject = new AutoResetEvent(false);
            client.Connected += (status) =>
            {
                Assert.Equal(AuthStatus.Authorized, status);
                waitObject.Set();
            };

            Assert.True(waitObject.WaitOne(
                TimeSpan.FromSeconds(10)));

            await client.DisconnectAsync();
        }
    }
}
