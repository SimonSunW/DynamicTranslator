using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicTranslator.Extensions;
using Newtonsoft.Json.Linq;

namespace DynamicTranslator.Google
{
	public class GoogleTranslateMeanOrganizer
	{
		public Task<string> OrganizeMean(string text, string fromLanguageExtension)
		{
			var result = text.DeserializeAs<Dictionary<string, object>>();
			var arrayTree = result["sentences"] as JArray;
			var output = arrayTree.GetFirstValueInArrayGraph<string>();
			return Task.FromResult(output);
		}
	}
}
