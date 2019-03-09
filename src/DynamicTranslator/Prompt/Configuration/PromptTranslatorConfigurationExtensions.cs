using System;
using DynamicTranslator.Configuration.Startup;

namespace DynamicTranslator.Prompt.Configuration
{
    public static class PromptTranslatorConfigurationExtensions
    {
        public static IPromptTranslatorConfiguration UsePromptTranslate(this ITranslatorModuleConfigurations moduleConfigurations)
        {
            moduleConfigurations.Configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Prompt);

            return moduleConfigurations.Configurations.Get<IPromptTranslatorConfiguration>();
        }

        public static void WithConfigurations(this IPromptTranslatorConfiguration configuration, Action<IPromptTranslatorConfiguration> creator)
        {
            creator(configuration);
        }
    }
}
