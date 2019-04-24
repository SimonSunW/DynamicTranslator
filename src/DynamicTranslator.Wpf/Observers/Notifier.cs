using DynamicTranslator.Wpf.ViewModel;

namespace DynamicTranslator.Wpf.Observers
{
    public class Notifier
    {
        private readonly GrowlNotifications _growlNotifications;

        public Notifier(GrowlNotifications growlNotifications)
        {
            _growlNotifications = growlNotifications;
        }

        public void AddNotification(string title, string imageUrl, string text)
        {
            _growlNotifications.AddNotification(new Notification { ImageUrl = imageUrl, Message = text, Title = title });
        }
    }
}
