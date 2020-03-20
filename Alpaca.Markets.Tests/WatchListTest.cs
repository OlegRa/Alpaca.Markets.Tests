using System;
using System.Linq;
using Xunit;

namespace Alpaca.Markets.Tests
{
    public sealed class WatchListTest : IDisposable
    {
        private readonly AlpacaTradingClient _restClient = ClientsFactory.GetAlpacaTradingClient();

        [Fact]
        public async void AllOperationsByIdWork()
        {
            var originalWatchListsList = await _restClient.ListWatchListsAsync();

            Assert.NotNull(originalWatchListsList);

            var newWatchListName = DateTime.UtcNow.ToString("O");

            var newWatchList = await _restClient.CreateWatchListAsync(
                new NewWatchListRequest(newWatchListName, new[] { "AAPL", "MSFT", "GOOG" }));

            Assert.NotNull(newWatchList);
            Assert.Equal(newWatchListName, newWatchList.Name);

            var updatedWatchList = await _restClient.GetWatchListByIdAsync(
                newWatchList.WatchListId);

            Assert.NotNull(updatedWatchList);
            Assert.Equal(newWatchList.Name, updatedWatchList.Name);
            Assert.Equal(newWatchList.Created, updatedWatchList.Created);
            Assert.Equal(newWatchList.Updated, updatedWatchList.Updated);
            Assert.Equal(newWatchList.WatchListId, updatedWatchList.WatchListId);
            Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);

            updatedWatchList = await _restClient.AddAssetIntoWatchListByIdAsync(
                new ChangeWatchListRequest<Guid>(newWatchList.WatchListId, "AMZN"));

            Assert.NotNull(updatedWatchList);
            Assert.Equal(newWatchList.Assets.Count + 1, updatedWatchList.Assets.Count);
            Assert.Contains(updatedWatchList.Assets,
                asset => String.Equals(asset.Symbol, "AMZN", StringComparison.Ordinal));

            updatedWatchList = await _restClient.DeleteAssetFromWatchListByIdAsync(
                new ChangeWatchListRequest<Guid>(newWatchList.WatchListId, "MSFT"));

            Assert.NotNull(updatedWatchList);
            Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);
            Assert.DoesNotContain(updatedWatchList.Assets,
                asset => String.Equals(asset.Symbol, "MSFT", StringComparison.Ordinal));

            var updatedWatchListName = newWatchListName + "_Updated";
            updatedWatchList = await _restClient.UpdateWatchListByIdAsync(
                new UpdateWatchListRequest(newWatchList.WatchListId, updatedWatchListName, new [] { "IBM" }));

            Assert.NotNull(updatedWatchList);
            Assert.Equal(updatedWatchListName, updatedWatchList.Name);

            Assert.Equal(1, updatedWatchList.Assets.Count);
            Assert.Equal("IBM", updatedWatchList.Assets.Single().Symbol);

            Assert.True(await _restClient.DeleteWatchListByIdAsync(newWatchList.WatchListId));
        }

        [Fact]
        public async void AllOperationsByNameWork()
        {
            var originalWatchListsList = await _restClient.ListWatchListsAsync();

            Assert.NotNull(originalWatchListsList);

            var newWatchListName = DateTime.UtcNow.ToString("O");

            var newWatchList = await _restClient.CreateWatchListAsync(
                new NewWatchListRequest(newWatchListName, new[] { "AAPL", "MSFT", "GOOG" }));

            Assert.NotNull(newWatchList);
            Assert.Equal(newWatchListName, newWatchList.Name);

            var updatedWatchList = await _restClient.GetWatchListByNameAsync(
                newWatchListName);

            Assert.NotNull(updatedWatchList);
            Assert.Equal(newWatchList.Name, updatedWatchList.Name);
            Assert.Equal(newWatchList.Created, updatedWatchList.Created);
            Assert.Equal(newWatchList.Updated, updatedWatchList.Updated);
            Assert.Equal(newWatchList.WatchListId, updatedWatchList.WatchListId);
            Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);


            updatedWatchList = await _restClient.AddAssetIntoWatchListByNameAsync(
                new ChangeWatchListRequest<String>(newWatchList.Name, "AMZN"));

            Assert.NotNull(updatedWatchList);
            Assert.Equal(newWatchList.Assets.Count + 1, updatedWatchList.Assets.Count);
            Assert.Contains(updatedWatchList.Assets,
                asset => String.Equals(asset.Symbol, "AMZN", StringComparison.Ordinal));

            updatedWatchList = await _restClient.DeleteAssetFromWatchListByNameAsync(
                new ChangeWatchListRequest<String>(newWatchList.Name, "MSFT"));

            Assert.NotNull(updatedWatchList);
            Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);
            Assert.DoesNotContain(updatedWatchList.Assets,
                asset => String.Equals(asset.Symbol, "MSFT", StringComparison.Ordinal));

            Assert.True(await _restClient.DeleteWatchListByNameAsync(newWatchList.Name));
        }

        public void Dispose() => _restClient?.Dispose();
    }
}
