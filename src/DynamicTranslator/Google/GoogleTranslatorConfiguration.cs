using System.Collections.Generic;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Google
{
	public class GoogleTranslatorConfiguration : AbstractTranslatorConfiguration
	{
		public override IList<Language> SupportedLanguages { get; set; }

		public override string Url { get; set; }

		public override TranslatorType TranslatorType => TranslatorType.Google;

		public GoogleTranslatorConfiguration(ActiveTranslatorConfiguration activeTranslatorConfiguration, ApplicationConfiguration applicationConfiguration) : base(activeTranslatorConfiguration, applicationConfiguration)
		{
		}
	}
}
