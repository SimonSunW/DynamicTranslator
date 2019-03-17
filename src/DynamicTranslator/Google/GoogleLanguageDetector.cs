using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using RestSharp;

namespace DynamicTranslator.Google
{
	public class GoogleLanguageDetector
	{
		private readonly DynamicTranslatorConfiguration _configuration;

		public GoogleLanguageDetector(DynamicTranslatorConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<string> DetectLanguage(string text)
		{
			var uri = string.Format(
				_configuration.GoogleTranslatorConfiguration.Url,
				_configuration.ApplicationConfiguration.ToLanguage.Extension,
				_configuration.ApplicationConfiguration.ToLanguage.Extension,
				HttpUtility.UrlEncode(text));

			var response = await _configuration.ClientFactory().With(client => { client.BaseUrl = new Uri(uri); })
				.ExecuteGetTaskAsync(new RestRequest(Method.GET)
					.AddHeader("Accept-Language", "en-US,en;q=0.8,tr;q=0.6")
					.AddHeader("Accept-Encoding", "gzip, deflate, sdch")
					.AddHeader("User-Agent",
						"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36")
					.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"));

			if (response.Ok())
			{
				var result = response.Content.DeserializeAs<Dictionary<string, object>>();
				return result?["src"]?.ToString();
			}

			return _configuration.ApplicationConfiguration.FromLanguage.Extension;
		}
	}
}