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

        public IAlpacaDataClient GetAlpacaDataClient() =>
            Environments.Paper.GetAlpacaDataClient(getSecretKey());

        public IAlpacaTradingClient GetAlpacaTradingClient() =>
            Environments.Paper.GetAlpacaTradingClient(getSecretKey());

        public IAlpacaStreamingClient GetAlpacaStreamingClient() =>
            Environments.Paper.GetAlpacaStreamingClient(getSecretKey());

        public IAlpacaDataStreamingClient GetAlpacaDataStreamingClient() =>
            Environments.Paper.GetAlpacaDataStreamingClient(getSecretKey());

        private SecretKey getSecretKey() => new SecretKey(_alpacaKeyId, _alpacaSecretKey);
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class LiveEnvironmentClientsFactoryFixture
    {
        private readonly String _alpacaKeyId;

        private readonly SecretKey _alpacaSecretKey;

        public LiveEnvironmentClientsFactoryFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile(Path.Combine(
                    Environment.CurrentDirectory, @"..\..\..\Development.json"), true)
                .Build();

            _alpacaKeyId = configuration["LIVE_ALPACA_KEY_ID"];
            
            var alpacaSecretKey = configuration["LIVE_ALPACA_SECRET_KEY"];

            _alpacaSecretKey = String.IsNullOrEmpty(alpacaSecretKey)
                ? new SecretKey(
                    configuration["PAPER_ALPACA_KEY_ID"],
                    configuration["PAPER_ALPACA_SECRET_KEY"])
                : new SecretKey(_alpacaKeyId, alpacaSecretKey);
        }

        public IPolygonDataClient GetPolygonDataClient() =>
            Environments.Live.GetPolygonDataClient(_alpacaKeyId);

        public IPolygonStreamingClient GetPolygonStreamingClient() =>
            Environments.Live.GetPolygonStreamingClient(_alpacaKeyId);

        public IAlpacaTradingClient GetAlpacaTradingClient() =>
            Environments.Live.GetAlpacaTradingClient(_alpacaSecretKey);
    }

    [CollectionDefinition("PaperEnvironment")]
    public sealed class PaperEnvironmentClientsFactoryCollection : ICollectionFixture<PaperEnvironmentClientsFactoryFixture> { }

    [CollectionDefinition("LiveEnvironment")]
    public sealed class LiveEnvironmentClientsFactoryCollection : ICollectionFixture<LiveEnvironmentClientsFactoryFixture> { }
}
