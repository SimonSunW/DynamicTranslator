using System.Reflection;
using Abp.Modules;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.SesliSozluk.Configuration;

namespace DynamicTranslator.SesliSozluk
{
    [DependsOn(typeof(DynamicTranslatorApplicationModule))]
    public class DynamicTranslatorSesliSozlukModule : DynamicTranslatorModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            Configurations.ModuleConfigurations.UseSesliSozlukTranslate().WithConfigurations(configuration =>
                          {
                              configuration.Url = "http://www.seslisozluk.net/c%C3%BCmle-%C3%A7eviri/";
                              configuration.SupportedLanguages = LanguageMapping.SesliSozluk.ToLanguages();
                          });
        }
    }
}
