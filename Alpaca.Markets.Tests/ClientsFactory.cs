using System;

namespace Alpaca.Markets.Tests
{
    internal static class ClientsFactory
    {
        private const String KEY_ID = "AKEW7ZBQUSNUHOJNQ5MS";

        private const String POLYGON_KEY_ID = KEY_ID + "-staging";

        private const String SECRET_KEY = "Yr2Tms89rQ6foRLNu4pz3w/yXOrxQGDmXctU1BCn";

        public static AlpacaDataClient GetAlpacaDataClient() =>
            Staging.Environment.GetAlpacaDataClient(new SecretKey(KEY_ID, SECRET_KEY));

        public static AlpacaTradingClient GetAlpacaTradingClient() =>
            Staging.Environment.GetAlpacaTradingClient(new SecretKey(KEY_ID, SECRET_KEY));

        public static PolygonDataClient GetPolygonDataClient() =>
            Staging.Environment.GetPolygonDataClient(POLYGON_KEY_ID);

        public static AlpacaStreamingClient GetAlpacaStreamingClient() =>
            Staging.Environment.GetAlpacaStreamingClient(new SecretKey(KEY_ID, SECRET_KEY));

        public static PolygonStreamingClient GetPolygonStreamingClient() =>
            Staging.Environment.GetPolygonStreamingClient(POLYGON_KEY_ID);
    }
}
