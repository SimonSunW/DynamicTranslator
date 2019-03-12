using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DynamicTranslator.Tureng
{
	public class TurengMeanOrganizer
	{
		public Task<string> OrganizeMean(string text, string fromLanguageExtension)
		{
			if (text == null)
			{
				return Task.FromResult(string.Empty);
			}

			string result = text;
			var output = new StringBuilder();
			var doc = new HtmlDocument();
			string decoded = WebUtility.HtmlDecode(result);
			doc.LoadHtml(decoded);

			if (!result.Contains("table") || doc.DocumentNode.SelectSingleNode("//table") == null)
			{
				return Task.FromResult(string.Empty);
			}

			(from x in doc.DocumentNode.Descendants()
			 where x.Name == "table"
			 from y in x.Descendants().AsParallel()
			 where y.Name == "tr"
			 from z in y.Descendants().AsParallel()
			 where (z.Name == "th" || z.Name == "td") && z.GetAttributeValue("lang", string.Empty) == (fromLanguageExtension == "tr" ? "en" : "tr")
			 from t in z.Descendants().AsParallel()
			 where t.Name == "a"
			 select t.InnerHtml)
				.AsParallel()
				.ToList()
				.ForEach(mean => output.AppendLine(mean));

			return Task.FromResult(output.ToString().ToLower().Trim());
		}
	}
}
