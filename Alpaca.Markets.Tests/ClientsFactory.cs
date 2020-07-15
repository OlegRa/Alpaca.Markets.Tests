using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Alpaca.Markets.Tests
{
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
                    Environment.CurrentDirectory, @"..\..\..\Development.json"), true)
                .Build();

            _alpacaKeyId = configuration["PAPER_ALPACA_KEY_ID"];
            _alpacaSecretKey = configuration["PAPER_ALPACA_SECRET_KEY"];
        }

        public AlpacaDataClient GetAlpacaDataClient() =>
            Environments.Paper.GetAlpacaDataClient(getSecretKey());

        public AlpacaTradingClient GetAlpacaTradingClient() =>
            Environments.Paper.GetAlpacaTradingClient(getSecretKey());

        public AlpacaStreamingClient GetAlpacaStreamingClient() =>
            Environments.Paper.GetAlpacaStreamingClient(getSecretKey());

        public AlpacaDataStreamingClient GetAlpacaDataStreamingClient() =>
            Environments.Paper.GetAlpacaDataStreamingClient(getSecretKey());

        private SecretKey getSecretKey() => new SecretKey(_alpacaKeyId, _alpacaSecretKey);
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class LiveEnvironmentClientsFactoryFixture
    {
        private readonly String _alpacaKeyId;

        private readonly String _alpacaSecretKey;

        public LiveEnvironmentClientsFactoryFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile(Path.Combine(
                    Environment.CurrentDirectory, @"..\..\..\Development.json"), true)
                .Build();

            _alpacaKeyId = configuration["LIVE_ALPACA_KEY_ID"];
            _alpacaSecretKey = configuration["LIVE_ALPACA_SECRET_KEY"];
        }

        public PolygonDataClient GetPolygonDataClient() =>
            Environments.Live.GetPolygonDataClient(_alpacaKeyId);

        public PolygonStreamingClient GetPolygonStreamingClient() =>
            Environments.Paper.GetPolygonStreamingClient(_alpacaKeyId);

        public AlpacaTradingClient GetAlpacaTradingClient() =>
            Environments.Live.GetAlpacaTradingClient(getSecretKey());

        private SecretKey getSecretKey() => new SecretKey(_alpacaKeyId, _alpacaSecretKey);
    }

    [CollectionDefinition("PaperEnvironment")]
    public sealed class PaperEnvironmentClientsFactoryCollection : ICollectionFixture<PaperEnvironmentClientsFactoryFixture> { }

    [CollectionDefinition("LiveEnvironment")]
    public sealed class LiveEnvironmentClientsFactoryCollection : ICollectionFixture<LiveEnvironmentClientsFactoryFixture> { }
}
