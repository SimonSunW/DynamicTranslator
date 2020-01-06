using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using DynamicTranslator.Configuration;
using DynamicTranslator.Google;
using DynamicTranslator.Model;

namespace DynamicTranslator.Wpf.Observers
{
    public class Finder
    {
        private readonly GoogleAnalyticsService _googleAnalytics;
        private readonly GoogleLanguageDetector _languageDetector;
        private readonly Notifier _notifier;
        private readonly WireUp _services;
        private string _previousString;

        public Finder(Notifier notifier,
            GoogleLanguageDetector languageDetector,
            WireUp services,
            GoogleAnalyticsService googleAnalytics)
        {
            _notifier = notifier;
            _languageDetector = languageDetector;
            _services = services;
            _googleAnalytics = googleAnalytics;
        }


        public async Task Find(EventPattern<WhenClipboardContainsTextEventArgs> value, CancellationToken cancellationToken)
        {
            try
            {
                var currentString = value.EventArgs.CurrentString;

                if (_previousString == currentString) return;

                _previousString = currentString;

                var fromLanguageExtension = await _languageDetector.DetectLanguage(currentString, cancellationToken);

                var results = await FindMeans(currentString, fromLanguageExtension, cancellationToken);
                var means = new ResultOrganizer().OrganizeResult(results, currentString, out var failedResults);
                Notify(currentString, means);

                await Trace(currentString, fromLanguageExtension);
            }
            catch (Exception ex)
            {
                Notify("Error", ex.Message);
            }
        }

        private Task<TranslateResult[]> FindMeans(string currentString, string fromLanguageExtension,
            CancellationToken cancellationToken)
        {
            var findFunc = _services
                .ActiveTranslatorConfiguration
                .ActiveTranslators
                .Select(x => x.Find(new TranslateRequest(currentString, fromLanguageExtension), cancellationToken))
                .ToList();

            return Task.WhenAll(findFunc.ToArray());
        }

        private async Task Trace(string currentString, string fromLanguageExtension)
        {
            await _googleAnalytics.TrackEventAsync("DynamicTranslator",
                "Translate",
                $"{currentString} | {fromLanguageExtension} - {_services.ApplicationConfiguration.ToLanguage.Extension} | v{ApplicationVersion.GetCurrentVersion()} ",
                null);

            await _googleAnalytics.TrackAppScreenAsync("DynamicTranslator",
                ApplicationVersion.GetCurrentVersion(),
                "dynamictranslator",
                "dynamictranslator",
                "notification");
        }

        private void Notify(string currentString, string means)
        {
            _notifier.AddNotification(currentString, ImageUrls.NotificationUrl, means);
        }
    }
}