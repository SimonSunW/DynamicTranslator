using DynamicTranslator.Google;
using DynamicTranslator.Prompt;
using DynamicTranslator.SesliSozluk;
using DynamicTranslator.Tureng;
using DynamicTranslator.Yandex;

namespace DynamicTranslator.Configuration.Startup
{
	public class DynamicTranslatorConfiguration
	{
		public ActiveTranslatorConfiguration ActiveTranslatorConfiguration { get; set; }

		public AppConfigManager AppConfigManager { get; set; }

		public ApplicationConfiguration ApplicationConfiguration { get; set; }

		public GoogleAnalyticsConfiguration GoogleAnalyticsConfiguration { get; set; }

		public GoogleTranslatorConfiguration GoogleTranslatorConfiguration { get; set; }

		public PromptTranslatorConfiguration PromptTranslatorConfiguration { get; set; }

		public TurengTranslatorConfiguration TurengTranslatorConfiguration { get; set; }

		public SesliSozlukTranslatorConfiguration SesliSozlukTranslatorConfiguration { get; set; }
		
		public YandexTranslatorConfiguration YandexTranslatorConfiguration { get; set; }
	}
}
