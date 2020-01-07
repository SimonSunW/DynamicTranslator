using System.Threading;
using System.Threading.Tasks;
using DynamicTranslator.Model;

namespace DynamicTranslator
{
    public interface ITranslator
    {
        Task<TranslateResult> Translate(TranslateRequest request, CancellationToken cancellationToken = default);
    }
}