using System;

namespace Alpaca.Markets.Tests
{
    internal sealed class Staging : IEnvironment
    {
        private Staging() {}

        public static IEnvironment Environment { get; } = new Staging();

        public Uri AlpacaTradingApi { get; } = new Uri("https://staging-api.tradetalk.us");

        public Uri AlpacaDataApi => Environments.Live.AlpacaDataApi;

        public Uri PolygonDataApi => Environments.Live.PolygonDataApi;

        public Uri AlpacaStreamingApi { get; } = new Uri("wss://staging-api.tradetalk.us/stream");

        public Uri PolygonStreamingApi => Environments.Live.PolygonStreamingApi;
    }
}
