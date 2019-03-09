using System.Threading.Tasks;
using DynamicTranslator.Extensions;
using DynamicTranslator.Orchestrators.Organizers;

namespace DynamicTranslator.Prompt.Orchestration
{
    public class PromptMeanOrganizer : AbstractMeanOrganizer
    {
        public override TranslatorType TranslatorType => TranslatorType.Prompt;

        public override Task<Maybe<string>> OrganizeMean(string text, string fromLanguageExtension)
        {
            var promptResult = text.DeserializeAs<PromptResult>();
            return Task.FromResult(new Maybe<string>(promptResult.d.result));
        }
    }
}
