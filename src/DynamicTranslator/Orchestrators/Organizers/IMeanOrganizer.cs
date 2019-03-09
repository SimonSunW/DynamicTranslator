using System.Threading.Tasks;

namespace DynamicTranslator.Orchestrators.Organizers
{
    public interface IMeanOrganizer : IMustHaveTranslatorType
    {
        Task<Maybe<string>> OrganizeMean(string text);

        Task<Maybe<string>> OrganizeMean(string text, string fromLanguageExtension);
    }
}
