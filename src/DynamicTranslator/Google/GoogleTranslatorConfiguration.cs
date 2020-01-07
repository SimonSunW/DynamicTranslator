using System;
using System.Collections.Generic;
using DynamicTranslator.Configuration;
using DynamicTranslator.Model;

namespace DynamicTranslator.Google
{
	public class GoogleTranslatorConfiguration : TranslatorConfiguration
	{
		public override IList<Language> SupportedLanguages { get; set; }

		public override string Url { get; set; }

		public override Type TranslatorType => typeof(GoogleTranslator);

		public GoogleTranslatorConfiguration(ActiveTranslatorConfiguration activeTranslatorConfiguration, ApplicationConfiguration applicationConfiguration) : base(activeTranslatorConfiguration, applicationConfiguration)
		{
		}
	}
}
