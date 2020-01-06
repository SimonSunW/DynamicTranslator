using System.Threading.Tasks;
using DynamicTranslator.Google;

namespace DynamicTranslator
{
    public class GoogleAnalyticsTracker
    {
        private readonly GoogleAnalyticsService _googleAnalyticsService;

        public GoogleAnalyticsTracker(GoogleAnalyticsService googleAnalyticsService)
        {
            _googleAnalyticsService = googleAnalyticsService;
        }

        public Task Track()
        {
            return _googleAnalyticsService.TrackAppScreenAsync("DynamicTranslator",
                ApplicationVersion.GetCurrentVersion(),
                "dynamictranslator",
                "dynamictranslator",
                "MainWindow");
        }
    }
}