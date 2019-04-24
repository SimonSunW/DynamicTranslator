using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DynamicTranslator.Configuration;
using DynamicTranslator.Google;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamicTranslator.Tests
{
    public class FinderTests
    {
        [Fact]
        public async Task Google_should_work()
        {
            var handler = new TestMessageHandler
            {
                Sender = message => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content =  new StringContent(JsonConvert.SerializeObject(
                        
                        new Dictionary<string,object>()
                        {
                            ["sentences"] = new JArray("Sehir")
                        }))
                })
            };
            var config = new DynamicTranslatorServices(builder =>
            {
                builder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ToLanguage", "Turkish"),
                    new KeyValuePair<string, string>("FromLanguage", "English"),
                });
            })
            {
                MessageHandler = handler
            };

            var google = new GoogleTranslator(config);
            config.ActiveTranslatorConfiguration.Activate(TranslatorType.Google);
            string result = await config.FindBy("city", TranslatorType.Google);
            result.Should().Be("Sehir");
        }
    }
}
