using System;
using System.Text;
using System.Threading.Tasks;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.Yandex
{
	public class YandexMeanFinder
	{
		private readonly ApplicationConfiguration _applicationConfiguration;
		private readonly YandexTranslatorConfiguration _configuration;
		private readonly YandexMeanOrganizer _meanOrganizer;

		private readonly IRestClient _restClient;

		public YandexMeanFinder(
			ApplicationConfiguration applicationConfiguration,
			IRestClient restClient, YandexTranslatorConfiguration configuration, YandexMeanOrganizer meanOrganizer)
		{
			_applicationConfiguration = applicationConfiguration;
			_restClient = restClient;
			_configuration = configuration;
			_meanOrganizer = meanOrganizer;
		}

		public async Task<TranslateResult> Find(TranslateRequest translateRequest)
		{
			var address = new Uri(string.Format(_configuration.Url +
			                                    new StringBuilder()
				                                    .Append($"key={_configuration.ApiKey}")
				                                    .Append(Headers.Ampersand)
				                                    .Append($"lang={translateRequest.FromLanguageExtension}-{_applicationConfiguration.ToLanguage.Extension}")
				                                    .Append(Headers.Ampersand)
				                                    .Append($"text={Uri.EscapeUriString(translateRequest.CurrentText)}")));

			IRestResponse response = await _restClient.Manipulate(client => { client.BaseUrl = address; }).ExecutePostTaskAsync(new RestRequest(Method.POST));


			string mean = string.Empty;
			if (response.Ok())
			{
				mean = await _meanOrganizer.OrganizeMean(response.Content);
			}

			return new TranslateResult(true, mean);
		}
	}
}
