using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DynamicTranslator.Configuration;
using DynamicTranslator.Google;
using DynamicTranslator.Yandex;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamicTranslator.Tests
{
    public class GoogleTests
    {
        private readonly DynamicTranslatorServices _services;

        public GoogleTests()
        {
            _services = new DynamicTranslatorServices(builder =>
             {
                 builder.AddInMemoryCollection(new[]
                 {
                    new KeyValuePair<string, string>("ToLanguage", "Turkish"),
                    new KeyValuePair<string, string>("FromLanguage", "English"),
                 });
             })
            {
                MessageHandler = GoogleMessageHandler()
            };
        }

        private static TestMessageHandler GoogleMessageHandler()
        {
            return new TestMessageHandler
            {
                Sender = message => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(

                        new Dictionary<string, object>()
                        {
                            ["sentences"] = new JArray("Sehir")
                        }))
                })
            };
        }

        private static TestMessageHandler YandexMessageHandler()
        {
            return new TestMessageHandler
            {
                Sender = message => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(

                        new Dictionary<string, object>()
                        {
                            ["sentences"] = new JArray("Sehir")
                        }))
                })
            };
        }

        [Fact]
        public async Task Google_should_work()
        {
            var google = new GoogleTranslator(_services);
            _services.ActiveTranslatorConfiguration.Activate(TranslatorType.Google);
            string result = await _services.FindBy("city", TranslatorType.Google);
            result.Should().Be("Sehir");
        }
    }
}
