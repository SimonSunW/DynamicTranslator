namespace DynamicTranslator.Wpf.Observers
{
    public interface INotifier
    {
        void AddNotification(string title, string imageUrl, string text);
    }
}