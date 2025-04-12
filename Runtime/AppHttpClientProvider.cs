using System.Net.Http;

namespace Drboum.Utilities
{
    public static class AppHttpClientProvider
    {
        private static HttpClient _client;

        public static HttpClient Client {
            get {
                _client ??= new();
                return _client;
            }
        }
    }
}