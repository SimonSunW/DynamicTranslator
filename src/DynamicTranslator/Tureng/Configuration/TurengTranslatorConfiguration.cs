using System.Collections.Generic;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Tureng.Configuration
{
    public class TurengTranslatorConfiguration : AbstractTranslatorConfiguration, ITurengTranslatorConfiguration
    {
        public override bool CanSupport()
        {
            return base.CanSupport() && ApplicationConfiguration.IsToLanguageTurkish;
        }

        public override IList<Language> SupportedLanguages { get; set; }

        public override string Url { get; set; }

        public override TranslatorType TranslatorType => TranslatorType.Tureng;
    }
}
