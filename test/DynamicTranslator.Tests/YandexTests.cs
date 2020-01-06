using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DynamicTranslator.Configuration;
using DynamicTranslator.Yandex;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace DynamicTranslator.Tests
{
    public class YandexTests
    {
        private readonly WireUp _services;

        public YandexTests()
        {
            _services = new WireUp(builder =>
             {
                 builder.AddInMemoryCollection(new[]
                 {
                    new KeyValuePair<string, string>("ToLanguage", "Turkish"),
                    new KeyValuePair<string, string>("FromLanguage", "English"),
                 });
             })
            {
                MessageHandler = YandexMessageHandler()
            };
        }

        private static TestMessageHandler YandexMessageHandler()
        {
            return new TestMessageHandler
            {
                Sender = message => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new YandexDetectResponse()
                    {
                        Text = new List<string>()
                        {
                            "Sehir"
                        }
                    }))
                })
            };
        }

        [Fact]
        public async Task Yandex_should_work()
        {
            var yandex = new YandexTranslator();
            _services.ActiveTranslatorConfiguration.Activate(TranslatorType.Yandex);
            string result = await _services.FindBy("city", TranslatorType.Yandex);
            result.Should().Be("Sehir".ToLower());
        }
    }
}
