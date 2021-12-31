namespace Alpaca.Markets.Tests;

public sealed partial class AlpacaTradingClientTest
{
    [Fact]
    public async void AllOperationsByIdWork()
    {
        var originalWatchListsList = await _alpacaTradingClient.ListWatchListsAsync();

        Assert.NotNull(originalWatchListsList);

        var newWatchListName = DateTime.UtcNow.ToString("O");

        var newWatchList = await _alpacaTradingClient.CreateWatchListAsync(
            new NewWatchListRequest(newWatchListName, new[] { "AAPL", "MSFT", "GOOG" }));

        Assert.NotNull(newWatchList);
        Assert.Equal(newWatchListName, newWatchList.Name);
        Assert.NotEqual(newWatchList.AccountId, Guid.Empty);

        var updatedWatchList = await _alpacaTradingClient.GetWatchListByIdAsync(
            newWatchList.WatchListId);

        Assert.NotNull(updatedWatchList);
        Assert.Equal(newWatchList.Name, updatedWatchList.Name);
        Assert.Equal(newWatchList.CreatedUtc, updatedWatchList.CreatedUtc);
        Assert.Equal(newWatchList.UpdatedUtc, updatedWatchList.UpdatedUtc);
        Assert.Equal(newWatchList.WatchListId, updatedWatchList.WatchListId);
        Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);

        updatedWatchList = await _alpacaTradingClient.AddAssetIntoWatchListByIdAsync(
            new ChangeWatchListRequest<Guid>(newWatchList.WatchListId, "AMZN"));

        Assert.NotNull(updatedWatchList);
        Assert.Equal(newWatchList.Assets.Count + 1, updatedWatchList.Assets.Count);
        Assert.Contains(updatedWatchList.Assets,
            asset => String.Equals(asset.Symbol, "AMZN", StringComparison.Ordinal));

        updatedWatchList = await _alpacaTradingClient.DeleteAssetFromWatchListByIdAsync(
            new ChangeWatchListRequest<Guid>(newWatchList.WatchListId, "MSFT"));

        Assert.NotNull(updatedWatchList);
        Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);
        Assert.DoesNotContain(updatedWatchList.Assets,
            asset => String.Equals(asset.Symbol, "MSFT", StringComparison.Ordinal));

        var updatedWatchListName = newWatchListName + "_Updated";
        updatedWatchList = await _alpacaTradingClient.UpdateWatchListByIdAsync(
            new UpdateWatchListRequest(newWatchList.WatchListId, updatedWatchListName, new [] { "IBM" }));

        Assert.NotNull(updatedWatchList);
        Assert.Equal(updatedWatchListName, updatedWatchList.Name);

        Assert.Equal(1, updatedWatchList.Assets.Count);
        Assert.Equal("IBM", updatedWatchList.Assets.Single().Symbol);

        Assert.True(await _alpacaTradingClient.DeleteWatchListByIdAsync(newWatchList.WatchListId));
    }

    [Fact]
    public async void AllOperationsByNameWork()
    {
        var originalWatchListsList = await _alpacaTradingClient.ListWatchListsAsync();

        Assert.NotNull(originalWatchListsList);

        var newWatchListName = DateTime.UtcNow.ToString("O");

        var newWatchList = await _alpacaTradingClient.CreateWatchListAsync(
            new NewWatchListRequest(newWatchListName, new[] { "AAPL", "MSFT", "GOOG" }));

        Assert.NotNull(newWatchList);
        Assert.Equal(newWatchListName, newWatchList.Name);

        var updatedWatchList = await _alpacaTradingClient.GetWatchListByNameAsync(
            newWatchListName);

        Assert.NotNull(updatedWatchList);
        Assert.Equal(newWatchList.Name, updatedWatchList.Name);
        Assert.Equal(newWatchList.CreatedUtc, updatedWatchList.CreatedUtc);
        Assert.Equal(newWatchList.UpdatedUtc, updatedWatchList.UpdatedUtc);
        Assert.Equal(newWatchList.WatchListId, updatedWatchList.WatchListId);
        Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);


        updatedWatchList = await _alpacaTradingClient.AddAssetIntoWatchListByNameAsync(
            new ChangeWatchListRequest<String>(newWatchList.Name, "AMZN"));

        Assert.NotNull(updatedWatchList);
        Assert.Equal(newWatchList.Assets.Count + 1, updatedWatchList.Assets.Count);
        Assert.Contains(updatedWatchList.Assets,
            asset => String.Equals(asset.Symbol, "AMZN", StringComparison.Ordinal));

        updatedWatchList = await _alpacaTradingClient.DeleteAssetFromWatchListByNameAsync(
            new ChangeWatchListRequest<String>(newWatchList.Name, "MSFT"));

        Assert.NotNull(updatedWatchList);
        Assert.Equal(newWatchList.Assets.Count, updatedWatchList.Assets.Count);
        Assert.DoesNotContain(updatedWatchList.Assets,
            asset => String.Equals(asset.Symbol, "MSFT", StringComparison.Ordinal));

        Assert.True(await _alpacaTradingClient.DeleteWatchListByNameAsync(newWatchList.Name));
    }
}
