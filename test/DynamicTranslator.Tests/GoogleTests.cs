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
        private readonly WireUp _wireUp;

        public GoogleTests()
        {
            _wireUp = new WireUp(builder =>
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

        [Fact]
        public async Task Google_should_work()
        {
           
        }
    }
}
