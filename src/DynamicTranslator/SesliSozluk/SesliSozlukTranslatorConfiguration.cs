using System.Collections.Generic;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.SesliSozluk
{
    public class SesliSozlukTranslatorConfiguration : AbstractTranslatorConfiguration
    {
        public override IList<Language> SupportedLanguages { get; set; }

        public override string Url { get; set; }

        public override TranslatorType TranslatorType => TranslatorType.SesliSozluk;

        public SesliSozlukTranslatorConfiguration(ActiveTranslatorConfiguration activeTranslatorConfiguration, ApplicationConfiguration applicationConfiguration) : base(activeTranslatorConfiguration, applicationConfiguration)
        {
        }
    }
}
