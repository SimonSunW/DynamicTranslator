using System;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.Tureng
{
    public class TurengMeanFinder
    {
        private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

        private readonly IRestClient _restClient;
        private readonly TurengTranslatorConfiguration _configuration;
        private readonly TurengMeanOrganizer _meanOrganizer;

        public TurengMeanFinder(IRestClient restClient, TurengTranslatorConfiguration configuration, TurengMeanOrganizer meanOrganizer)
        {
	        _restClient = restClient;
	        _configuration = configuration;
	        _meanOrganizer = meanOrganizer;
        }

        protected  async Task<TranslateResult> Find(TranslateRequest translateRequest)
        {
            var uri = new Uri(_configuration.Url + translateRequest.CurrentText);

            IRestResponse response = await _restClient
                .Manipulate(client =>
                {
                    client.BaseUrl = uri;
                    client.Encoding = Encoding.UTF8;
                    client.CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.FromHours(1));
                }).ExecuteGetTaskAsync(
                    new RestRequest(Method.GET)
                        .AddHeader(Headers.UserAgent, UserAgent)
                        .AddHeader(Headers.AcceptLanguage, AcceptLanguage));

            var mean = string.Empty;

            if (response.Ok())
            {
                mean = await _meanOrganizer.OrganizeMean(response.Content, translateRequest.FromLanguageExtension);
            }

            return new TranslateResult(true, mean);
        }
    }
}
