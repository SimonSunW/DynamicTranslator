using System.Reflection;

namespace DynamicTranslator
{
    public class DynamicTranslatorDomainModule : DynamicTranslatorModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
