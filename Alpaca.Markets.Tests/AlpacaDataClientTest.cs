using System;
using System.Net;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class AlpacaDataClientTest : IDisposable
    {
        private readonly AlpacaTradingClient _alpacaTradingClient = ClientsFactory.GetAlpacaTradingClient();

        [Fact]
        public async void GetPortfolioHistoryAsyncWorks()
        {
            var portfolioHistory = await _alpacaTradingClient.GetPortfolioHistoryAsync();

            Assert.NotNull(portfolioHistory);
            Assert.NotNull(portfolioHistory.Items);
        }

        [Fact]
        public async void GetAssetAsyncThrowsCustomException()
        {
            var exception = await Assert.ThrowsAsync<RestClientErrorException>(
                () => _alpacaTradingClient.GetAssetAsync("HEI-A"));

            Assert.NotNull(exception);
            Assert.NotNull(exception.Message);
            Assert.NotEqual(0, exception.ErrorCode);
        }

        public void Dispose() => _alpacaTradingClient?.Dispose();
    }
}
