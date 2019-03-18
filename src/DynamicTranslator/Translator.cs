namespace DynamicTranslator
{
	public class Translator
	{
		public Translator(string name, TranslatorType type, bool isEnabled, Find find)
		{
			IsEnabled = isEnabled;
			Name = name;
			Type = type;
			Find = find;
		}

		public Translator(string name, TranslatorType type, Find find) : this(name, type, true, find)
		{
			Name = name;
			Type = type;
		}

		public Translator Activate()
		{
			IsActive = true;
			return this;
		}

		public Translator DeActivate()
		{
			IsActive = false;
			return this;
		}

		public Find Find { get; }

		public bool IsActive { get; private set; }

		public bool IsEnabled { get; private set; }

		public string Name { get; private set; }

		public TranslatorType Type { get; private set; }
	}
}
