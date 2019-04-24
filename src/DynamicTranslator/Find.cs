using System.Threading;
using System.Threading.Tasks;
using DynamicTranslator.Model;

namespace DynamicTranslator
{
	public delegate Task<TranslateResult> Find(TranslateRequest request, CancellationToken cancellationToken = default);
}
