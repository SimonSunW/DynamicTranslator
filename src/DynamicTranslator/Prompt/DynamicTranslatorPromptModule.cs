using System;
using System.Threading.Tasks;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.Prompt
{

	public class DynamicTranslatorPromptModule
	{
		private const string ContentType = "application/json;Charset=UTF-8";
		private const string ContentTypeName = "Content-Type";

		public DynamicTranslatorPromptModule(DynamicTranslatorConfiguration configurations)
		{
			var cfg = new PromptTranslatorConfiguration(configurations.ActiveTranslatorConfiguration,
				configurations.ApplicationConfiguration)
			{
				Url = Url,
				Limit = CharacterLimit,
				Template = Template,
				Ts = Ts,
				SupportedLanguages = LanguageMapping.Prompt.ToLanguages()
			};

			configurations.PromptTranslatorConfiguration = cfg;

			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Prompt, async (translateRequest, token) =>
			{
				var requestObject = new
				{
					dirCode = $"{translateRequest.FromLanguageExtension}-{configurations.ApplicationConfiguration.ToLanguage.Extension}",
					template = cfg.Template,
					text = translateRequest.CurrentText,
					lang = translateRequest.FromLanguageExtension,
					limit = cfg.Limit,
					useAutoDetect = true,
					key = string.Empty,
					ts = cfg.Ts,
					tid = string.Empty,
					IsMobile = false
				};

				IRestResponse response = await configurations.ClientFactory()
					.With(client => { client.BaseUrl = cfg.Url.ToUri(); }).ExecutePostTaskAsync(new RestRequest(Method.POST)
						.AddHeader(ContentTypeName, ContentType)
						.AddParameter(ContentType, requestObject.ToJsonString(false), ParameterType.RequestBody));

				string mean = string.Empty;

				if (response.Ok())
				{
					mean = OrganizeMean(response.Content);
				}

				return new TranslateResult(true, mean);

			});
		}

		private string OrganizeMean(string text)
		{
			var promptResult = text.DeserializeAs<PromptResult>();
			return promptResult.d.result;
		}

		private const int CharacterLimit = 3000;
		private const string Template = "auto";
		private const string Ts = "MainSite";
		private const string Url = "http://www.online-translator.com/services/TranslationService.asmx/GetTranslateNew";

	}
}
