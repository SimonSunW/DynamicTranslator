using System.Threading.Tasks;
using System.Xml;
using DynamicTranslator.Extensions;

namespace DynamicTranslator.Yandex
{
	public class YandexMeanOrganizer
	{
		public Task<string> OrganizeMean(string text)
		{
			string output;

			if (text == null)
			{
				return Task.FromResult(string.Empty);
			}

			if (text.IsXml())
			{
				var doc = new XmlDocument();
				doc.LoadXml(text);
				XmlNode node = doc.SelectSingleNode("//Translation/text");
				output = node?.InnerText ?? "!!! An error occurred";
			}
			else
			{
				output = text.DeserializeAs<YandexDetectResponse>().Text.JoinAsString(",");
			}

			return Task.FromResult(output.ToLower().Trim());
		}
	}
}
