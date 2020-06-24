using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Alpaca.Markets.Tests
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class ClientsFactoryFixture
    {
        private readonly String _alpacaKeyId;

        private readonly String _polygonKeyId;

        private readonly String _alpacaSecretKey;

        public ClientsFactoryFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile(Path.Combine(
                    Environment.CurrentDirectory, @"..\..\..\Development.json"), true)
                .Build();

            _alpacaKeyId = configuration["ALPACA_KEY_ID"];
            _polygonKeyId = configuration["POLYGON_KEY_ID"];
            _alpacaSecretKey = configuration["ALPACA_SECRET_KEY"];
        }

        public PolygonDataClient GetPolygonDataClient() =>
            Staging.Environment.GetPolygonDataClient(_polygonKeyId);

        public AlpacaDataClient GetAlpacaDataClient() =>
            Staging.Environment.GetAlpacaDataClient(getSecretKey());

        public AlpacaTradingClient GetAlpacaTradingClient() =>
            Staging.Environment.GetAlpacaTradingClient(getSecretKey());

        public AlpacaStreamingClient GetAlpacaStreamingClient() =>
            Staging.Environment.GetAlpacaStreamingClient(getSecretKey());

        public PolygonStreamingClient GetPolygonStreamingClient() =>
            Staging.Environment.GetPolygonStreamingClient(_polygonKeyId);

        public AlpacaDataStreamingClient GetAlpacaDataStreamingClient() =>
            Staging.Environment.GetAlpacaDataStreamingClient(getSecretKey());

        private SecretKey getSecretKey() => new SecretKey(_alpacaKeyId, _alpacaSecretKey);
    }
    
    [CollectionDefinition("Alpaca.Markets.Tests")]
    public sealed class ClientsFactoryCollection : ICollectionFixture<ClientsFactoryFixture> { }
}
