using System;
using System.Threading.Tasks;
using DynamicTranslator.Google;

namespace DynamicTranslator
{
    public class GoogleAnalyticsTracker : IObserver<long>
    {
        private readonly GoogleAnalyticsService _googleAnalyticsService;

        public GoogleAnalyticsTracker(GoogleAnalyticsService googleAnalyticsService)
        {
            _googleAnalyticsService = googleAnalyticsService;
        }

        public void OnCompleted() {}

        public void OnError(Exception error) {}

        public void OnNext(long value)
        {
            Task.Run(() =>
            {
                _googleAnalyticsService.TrackAppScreenAsync("DynamicTranslator",
                    ApplicationVersion.GetCurrentVersion(),
                    "dynamictranslator",
                    "dynamictranslator",
                    "MainWindow");
            });
        }
    }
}
