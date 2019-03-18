using System.Collections.Generic;
using System.Linq;
using DynamicTranslator.Extensions;

namespace DynamicTranslator.Configuration
{
	public class ActiveTranslatorConfiguration
	{
		public ActiveTranslatorConfiguration()
		{
			Translators = new List<Translator>();
		}

		public void Activate(TranslatorType translatorType)
		{
			Translators.FirstOrDefault(t => t.Type == translatorType)?.Activate();
		}

		public void AddTranslator(TranslatorType translatorType, Find find)
		{
			Translators.Add(new Translator(translatorType.ToString(), translatorType, find));
		}

		public void DeActivate()
		{
			Translators.ForEach(t => t.DeActivate());
		}

		public IReadOnlyList<Translator> ActiveTranslators => Translators.Where(x => x.IsActive).ToList();

		public IList<Translator> Translators { get; }
	}
}