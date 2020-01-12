using System;

namespace Alpaca.Markets.Tests
{
    internal static class ClientsFactory
    {
        private const String KEY_ID = "AKEW7ZBQUSNUHOJNQ5MS";

        private const String SECRET_KEY = "Yr2Tms89rQ6foRLNu4pz3w/yXOrxQGDmXctU1BCn";

        public static RestClient GetRestClient() =>
            new RestClient(
                new RestfulApiClientConfiguration
                {
                    KeyId = KEY_ID,
                    SecurityId = new SecretKey(SECRET_KEY),
                    DataApiUrl = Environments.Staging.AlpacaDataApi,
                    TradingApiUrl = Environments.Staging.AlpacaTradingApi
                });

        public static AlpacaStreamingClient GetAlpacaStreamingClient() =>
            new AlpacaStreamingClient(
                new AlpacaStreamingClientConfiguration()
                {
                    KeyId = KEY_ID,
                    SecretKey = SECRET_KEY,
                    ApiEndpoint = Environments.Staging.AlpacaStreamingApi
                });

        public static PolygonStreamingClient GetPolygonStreamingClient() =>
            new PolygonStreamingClient(
                new PolygonStreamingClientConfiguration()
                {
                    KeyId = KEY_ID,
                    ApiEndpoint = Environments.Staging.PolygonStreamingApi
                });
    }
}
