using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Yandex
{
	public class DynamicTranslatorYandexModule
	{
		private const string AnonymousUrl = "https://translate.yandex.net/api/v1/tr.json/translate?";
		private const string BaseUrl = "https://translate.yandex.com/";
		private const string InternalSId = "id=93bdaee7.57bb46e3.e787b736-0-0";
		private const string Url = "https://translate.yandex.net/api/v1.5/tr/translate?";

		public DynamicTranslatorYandexModule(DynamicTranslatorConfiguration configurations)
		{
			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Yandex);

			var yandex = new YandexTranslatorConfiguration(configurations.ActiveTranslatorConfiguration,
				configurations.ApplicationConfiguration)
			{
				BaseUrl = BaseUrl,
				SId = InternalSId,
				Url = Url,
				ApiKey = configurations.AppConfigManager.Get("YandexApiKey"),
				SupportedLanguages = LanguageMapping.Yandex.ToLanguages()
			};

			configurations.YandexTranslatorConfiguration = yandex;
		}
	}
}
