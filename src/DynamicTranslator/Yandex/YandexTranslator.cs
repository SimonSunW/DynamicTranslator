using System;
using System.Text;
using System.Xml;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.Yandex
{
	public class YandexTranslator
	{
		private const string AnonymousUrl = "https://translate.yandex.net/api/v1/tr.json/translate?";
		private const string BaseUrl = "https://translate.yandex.com/";
		private const string InternalSId = "id=93bdaee7.57bb46e3.e787b736-0-0";
		private const string Url = "https://translate.yandex.net/api/v1.5/tr/translate?";

		public YandexTranslator(DynamicTranslatorConfiguration configurations)
		{
			var yandex = new YandexTranslatorConfiguration(configurations.ActiveTranslatorConfiguration,
				configurations.ApplicationConfiguration)
			{
				BaseUrl = BaseUrl,
				SId = InternalSId,
				Url = Url,
				ApiKey = configurations.AppConfigManager.Get("YandexApiKey"),
				SupportedLanguages = LanguageMapping.Yandex.ToLanguages()
			};

			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Yandex, async (request, token) =>
			{

				var address = new Uri(string.Format(yandex.Url +
													new StringBuilder()
														.Append($"key={yandex.ApiKey}")
														.Append(Headers.Ampersand)
														.Append($"lang={request.FromLanguageExtension}-{configurations.ApplicationConfiguration.ToLanguage.Extension}")
														.Append(Headers.Ampersand)
														.Append($"text={Uri.EscapeUriString(request.CurrentText)}")));

				IRestResponse response = await configurations.ClientFactory().With(client => { client.BaseUrl = address; }).ExecutePostTaskAsync(new RestRequest(Method.POST));


				string mean = string.Empty;
				if (response.Ok())
				{
					mean = MakeMeaningful(mean);
				}

				return new TranslateResult(true, mean);

			});

			configurations.YandexTranslatorConfiguration = yandex;
		}

		private string MakeMeaningful(string text)
		{
			string output;
			if (text == null)
			{
				return string.Empty;

			}
			if (text.IsXml())
			{
				var doc = new XmlDocument();
				doc.LoadXml(text);
				XmlNode node = doc.SelectSingleNode("//Translation/text");
				output = node?.InnerText ?? "!!! An error occurred";
			}
			else
			{
				output = text.DeserializeAs<YandexDetectResponse>().Text.JoinAsString(",");
			}

			return output.ToLower().Trim();
		}
	}
}
