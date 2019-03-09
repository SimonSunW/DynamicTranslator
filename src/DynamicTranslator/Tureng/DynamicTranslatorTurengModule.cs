using System.Reflection;
using Abp.Modules;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.Tureng.Configuration;

namespace DynamicTranslator.Tureng
{
    [DependsOn(typeof(DynamicTranslatorApplicationModule))]
    public class DynamicTranslatorTurengModule : DynamicTranslatorModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            Configurations.ModuleConfigurations.UseSesliSozlukTranslate().WithConfigurations(configuration =>
            {
                configuration.Url = "http://tureng.com/search/";
                configuration.SupportedLanguages = LanguageMapping.Tureng.ToLanguages();
            });
        }
    }
}
