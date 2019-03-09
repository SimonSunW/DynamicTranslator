using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicTranslator.Model;

namespace DynamicTranslator.Orchestrators.Organizers
{
    public interface IResultOrganizer
    {
        Task<Maybe<string>> OrganizeResult(ICollection<TranslateResult> findedMeans, string currentString, out Maybe<string> failedResults);
    }
}
