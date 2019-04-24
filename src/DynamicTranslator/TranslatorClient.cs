using System.Net.Http;

namespace DynamicTranslator
{
    public class TranslatorClient
    {
        public TranslatorClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }
}