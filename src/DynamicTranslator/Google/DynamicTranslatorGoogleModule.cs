using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Google
{
	public class DynamicTranslatorGoogleModule
	{
		private const string GoogleTranslateUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={0}&hl={1}&dt=t&dt=bd&dj=1&source=bubble&q={2}";

		public DynamicTranslatorGoogleModule(DynamicTranslatorConfiguration configurations)
		{
			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Google);
			var googleTranslate = new GoogleTranslatorConfiguration(configurations.ActiveTranslatorConfiguration, configurations.ApplicationConfiguration)
			{
				Url = GoogleTranslateUrl,
				SupportedLanguages = LanguageMapping.All.ToLanguages()
			};

			configurations.GoogleTranslatorConfiguration = googleTranslate;
		}
	}
}
