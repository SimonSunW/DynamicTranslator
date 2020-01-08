using System;
using System.Collections.Generic;
using DynamicTranslator.Configuration;
using DynamicTranslator.Model;

namespace DynamicTranslator.SesliSozluk
{
    public class SesliSozlukTranslatorConfiguration : TranslatorConfiguration
    {
        public override IList<Language> SupportedLanguages { get; set; }

        public override string Url { get; set; }

        public override Type TranslatorType => typeof(SesliSozlukTranslator);

        public SesliSozlukTranslatorConfiguration(ActiveTranslatorConfiguration activeTranslatorConfiguration,
            ApplicationConfiguration applicationConfiguration) : base(activeTranslatorConfiguration,
            applicationConfiguration)
        {
        }
    }
}