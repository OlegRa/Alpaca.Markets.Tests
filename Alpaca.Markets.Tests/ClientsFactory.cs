using Microsoft.Extensions.Configuration;

namespace Alpaca.Markets.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class PaperEnvironmentClientsFactoryFixture
{
    private readonly String _alpacaKeyId;

    private readonly String _alpacaSecretKey;

    public PaperEnvironmentClientsFactoryFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile(Path.Combine(
                Environment.CurrentDirectory, @"..\..\..\..\Development.json"), true)
            .Build();

        _alpacaKeyId = configuration["PAPER_ALPACA_KEY_ID"] ?? String.Empty;
        _alpacaSecretKey = configuration["PAPER_ALPACA_SECRET_KEY"] ?? String.Empty;

        if (String.IsNullOrEmpty(_alpacaKeyId))
        {
            _alpacaKeyId = Guid.NewGuid().ToString("N");
        }
    }

    public IAlpacaDataClient GetAlpacaDataClient() =>
        Environments.Paper.GetAlpacaDataClient(getSecretKey());

    public IAlpacaTradingClient GetAlpacaTradingClient() =>
        Environments.Paper.GetAlpacaTradingClient(getSecretKey());

    public IAlpacaStreamingClient GetAlpacaStreamingClient() =>
        Environments.Paper.GetAlpacaStreamingClient(getSecretKey());

    public IAlpacaCryptoDataClient GetAlpacaCryptoDataClient() =>
        Environments.Paper.GetAlpacaCryptoDataClient(getSecretKey());

    public IAlpacaDataStreamingClient GetAlpacaDataStreamingClient() =>
        Environments.Paper.GetAlpacaDataStreamingClient(getSecretKey());

    public IAlpacaCryptoStreamingClient GetAlpacaCryptoStreamingClient() =>
        Environments.Paper.GetAlpacaCryptoStreamingClient(getSecretKey());

    private SecretKey getSecretKey() => new (_alpacaKeyId, _alpacaSecretKey);
}

[CollectionDefinition(nameof(PaperEnvironmentClientsFactoryCollection))]
public sealed class PaperEnvironmentClientsFactoryCollection
    : ICollectionFixture<PaperEnvironmentClientsFactoryFixture>;
