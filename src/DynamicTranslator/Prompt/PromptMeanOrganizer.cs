using System.Threading.Tasks;
using DynamicTranslator.Extensions;

namespace DynamicTranslator.Prompt
{
	public class PromptMeanOrganizer
	{
		public Task<string> OrganizeMean(string text)
		{
			var promptResult = text.DeserializeAs<PromptResult>();
			return Task.FromResult(promptResult.d.result);
		}
	}
}
