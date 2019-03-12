using System;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Extensions;
using RestSharp;

namespace DynamicTranslator.Yandex
{
	public class YandexLanguageDetector
	{
		private readonly ApplicationConfiguration _applicationConfiguration;

		private readonly YandexTranslatorConfiguration _configuration;

		public YandexLanguageDetector(YandexTranslatorConfiguration configuration,
			ApplicationConfiguration applicationConfiguration)
		{
			_configuration = configuration;
			_applicationConfiguration = applicationConfiguration;
		}

		public async Task<string> DetectLanguage(string text)
		{
			string uri = string.Format(_configuration.Url, text);

			IRestResponse response = await new RestClient(uri)
			{
				Encoding = Encoding.UTF8,
				CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.FromHours(1))
			}.ExecuteGetTaskAsync(new RestRequest(Method.GET)
				.AddHeader(Headers.CacheControl, Headers.NoCache)
				.AddHeader(Headers.AcceptLanguage, Headers.AcceptLanguageDefinition)
				.AddHeader(Headers.AcceptEncoding, Headers.AcceptEncodingDefinition)
				.AddHeader(Headers.Accept, "*/*")
				.AddHeader(Headers.UserAgent, Headers.UserAgentDefinition));

			var result = new YandexDetectResponse();

			if (response.Ok())
			{
				result = response.Content.DeserializeAs<YandexDetectResponse>();
			}

			if ((result != null) && string.IsNullOrEmpty(result.Lang))
			{
				return result.Lang;
			}

			return _applicationConfiguration.ToLanguage.Extension;
		}
	}
}
