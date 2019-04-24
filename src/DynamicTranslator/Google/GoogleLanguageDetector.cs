using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;

namespace DynamicTranslator.Google
{
    public class GoogleLanguageDetector
    {
        private readonly DynamicTranslatorServices _services;

        public GoogleLanguageDetector(DynamicTranslatorServices services)
        {
            _services = services;
        }

        public async Task<string> DetectLanguage(string text)
        {
            string uri = string.Format(
                _services.GoogleTranslatorConfiguration.Url,
                _services.ApplicationConfiguration.ToLanguage.Extension,
                _services.ApplicationConfiguration.ToLanguage.Extension,
                HttpUtility.UrlEncode(text));

            HttpClient httpClient = _services.ClientFactory().HttpClient;
            var request = new HttpRequestMessage {Method = HttpMethod.Get};
            request.Headers.Add(Headers.AcceptLanguage, "en-US,en;q=0.8,tr;q=0.6");
            request.Headers.Add(Headers.AcceptEncoding, "gzip, deflate, sdch");
            request.Headers.Add(Headers.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36");
            request.Headers.Add(Headers.Accept, "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.RequestUri = uri.ToUri();
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return _services.ApplicationConfiguration.FromLanguage.Extension;
            }

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var textReader = new StreamReader(stream))
            {
                var c = await textReader.ReadToEndAsync();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = content.DeserializeAs<Dictionary<string, object>>();
            return result?["src"]?.ToString();
        }
    }
}