using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Tureng
{
	public class DynamicTranslatorTurengModule
	{
		public DynamicTranslatorTurengModule(DynamicTranslatorConfiguration configurations)
		{
			var tureng = new TurengTranslatorConfiguration(configurations.ActiveTranslatorConfiguration, configurations.ApplicationConfiguration);
			tureng.Url = "http://tureng.com/search/";
			tureng.SupportedLanguages = LanguageMapping.Tureng.ToLanguages();

			configurations.TurengTranslatorConfiguration = tureng;
		}
	}
}
