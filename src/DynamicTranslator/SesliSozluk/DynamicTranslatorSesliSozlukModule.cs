using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.SesliSozluk
{
	public class DynamicTranslatorSesliSozlukModule
	{
		public DynamicTranslatorSesliSozlukModule(DynamicTranslatorConfiguration configurations)
		{
			var cfg = new SesliSozlukTranslatorConfiguration(configurations.ActiveTranslatorConfiguration, configurations.ApplicationConfiguration);
			cfg.Url = "http://www.seslisozluk.net/c%C3%BCmle-%C3%A7eviri/";
			cfg.SupportedLanguages = LanguageMapping.SesliSozluk.ToLanguages();

			configurations.SesliSozlukTranslatorConfiguration = cfg;
		}
	}
}
