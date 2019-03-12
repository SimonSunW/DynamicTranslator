using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Prompt
{

	public class DynamicTranslatorPromptModule
	{
		public DynamicTranslatorPromptModule(DynamicTranslatorConfiguration configurations)
		{
			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Prompt);
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
		}

		private const int CharacterLimit = 3000;
		private const string Template = "auto";
		private const string Ts = "MainSite";
		private const string Url = "http://www.online-translator.com/services/TranslationService.asmx/GetTranslateNew";
	}
}
