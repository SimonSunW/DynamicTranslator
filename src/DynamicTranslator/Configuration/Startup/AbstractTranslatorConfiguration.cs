using System.Collections.Generic;
using System.Linq;
using DynamicTranslator.LanguageManagement;

namespace DynamicTranslator.Configuration.Startup
{
    public abstract class AbstractTranslatorConfiguration
    {
	    private readonly ActiveTranslatorConfiguration _activeTranslatorConfiguration;
	    private readonly ApplicationConfiguration _applicationConfiguration;
	    public AbstractTranslatorConfiguration(ActiveTranslatorConfiguration activeTranslatorConfiguration, ApplicationConfiguration applicationConfiguration)
	    {
		    _activeTranslatorConfiguration = activeTranslatorConfiguration;
		    _applicationConfiguration = applicationConfiguration;
	    }

        public virtual bool CanSupport()
        {
            return SupportedLanguages.Any(x => x.Extension == _applicationConfiguration.ToLanguage.Extension);
        }

        public virtual bool IsActive()
        {
            return _activeTranslatorConfiguration.ActiveTranslators
                                                .Any(x => (x.Type == TranslatorType)
                                                          && x.IsActive
                                                          && x.IsEnabled);
        }

        public abstract IList<Language> SupportedLanguages { get; set; }

        public abstract string Url { get; set; }

        public abstract TranslatorType TranslatorType { get; }
    }
}
