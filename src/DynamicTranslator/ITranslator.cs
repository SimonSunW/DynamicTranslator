namespace DynamicTranslator
{
	public interface ITranslator
	{
		bool IsActive { get; }

		bool IsEnabled { get; }

		string Name { get; }

		TranslatorType Type { get; }

		ITranslator Activate();

		ITranslator DeActivate();

		Find Find { get; }
	}
}
