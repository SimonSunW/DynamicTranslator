using DynamicTranslator.Configuration.Startup;

namespace DynamicTranslator.Prompt.Configuration
{
    public interface IPromptTranslatorConfiguration : ITranslatorConfiguration, IConfiguration
    {
        string Template { get; set; }

        int Limit { get; set; }

        string Ts { get; set; }
    }
}
