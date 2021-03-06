﻿using System;
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

        public IAlpacaDataStreamingClient GetAlpacaDataStreamingClient() =>
            Environments.Paper.GetAlpacaDataStreamingClient(getSecretKey());

        private SecretKey getSecretKey() => new SecretKey(_alpacaKeyId, _alpacaSecretKey);
    }

    [CollectionDefinition("PaperEnvironment")]
    public sealed class PaperEnvironmentClientsFactoryCollection : ICollectionFixture<PaperEnvironmentClientsFactoryFixture> { }
}
