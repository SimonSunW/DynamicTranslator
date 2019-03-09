using System.Reflection;
using Abp.Modules;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.Prompt.Configuration;

namespace DynamicTranslator.Prompt
{
    [DependsOn(typeof(DynamicTranslatorApplicationModule))]
    public class DynamicTranslatorPromptModule : DynamicTranslatorModule
    {
        private const int CharacterLimit = 3000;
        private const string Template = "auto";
        private const string Ts = "MainSite";
        private const string Url = "http://www.online-translator.com/services/TranslationService.asmx/GetTranslateNew";

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            Configurations.ModuleConfigurations.UsePromptTranslate().WithConfigurations(configuration =>
                          {
                              configuration.Url = Url;
                              configuration.Limit = CharacterLimit;
                              configuration.Template = Template;
                              configuration.Ts = Ts;
                              configuration.SupportedLanguages = LanguageMapping.Prompt.ToLanguages();
                          });
        }
    }
}
