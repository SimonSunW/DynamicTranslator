using System.Threading.Tasks;
using DynamicTranslator.Wpf.ViewModel;

namespace DynamicTranslator.Wpf.Notification
{
    public class Notifier : INotifier
    {
        private readonly GrowlNotifications _growlNotifications;

        public Notifier(GrowlNotifications growlNotifications)
        {
            _growlNotifications = growlNotifications;
        }

        public void AddNotification(string title, string imageUrl, string text)
        {
            _growlNotifications.AddNotification(new ViewModel.Notification { ImageUrl = imageUrl, Message = text, Title = title });
        }

        public Task AddNotificationAsync(string title, string imageUrl, string text)
        {
            return _growlNotifications.AddNotificationAsync(new ViewModel.Notification { ImageUrl = imageUrl, Message = text, Title = title });
        }
    }
}
