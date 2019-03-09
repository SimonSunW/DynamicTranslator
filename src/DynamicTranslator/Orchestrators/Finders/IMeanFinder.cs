using System.Threading.Tasks;
using DynamicTranslator.Model;
using DynamicTranslator.Requests;

namespace DynamicTranslator.Orchestrators.Finders
{
    public interface IMeanFinder
    {
        Task<TranslateResult> FindMean(TranslateRequest translateRequest);
    }
}
