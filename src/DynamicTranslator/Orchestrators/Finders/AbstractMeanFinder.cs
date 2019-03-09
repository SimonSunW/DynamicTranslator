using System.Threading.Tasks;
using Abp.Dependency;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Model;
using DynamicTranslator.Orchestrators.Organizers;
using DynamicTranslator.Requests;

namespace DynamicTranslator.Orchestrators.Finders
{
    public abstract class AbstractMeanFinder<TConfiguration, TMeanOrganizer> : IMeanFinder, ITransientDependency
        where TConfiguration : ITranslatorConfiguration
        where TMeanOrganizer : IMeanOrganizer
    {
        public TConfiguration Configuration { get; set; }

        public TMeanOrganizer MeanOrganizer { get; set; }

        public virtual async Task<TranslateResult> FindMean(TranslateRequest translateRequest)
        {
            if (!Configuration.CanSupport())
            {
                return new TranslateResult(false, new Maybe<string>());
            }

            if (!Configuration.IsActive())
            {
                return new TranslateResult(false, new Maybe<string>());
            }

            return await Find(translateRequest);
        }

        protected abstract Task<TranslateResult> Find(TranslateRequest translateRequest);
    }
}
