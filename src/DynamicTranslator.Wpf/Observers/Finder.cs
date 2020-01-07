﻿using System;
using System.Collections.Generic;
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
        private readonly ActiveTranslatorConfiguration _activeTranslatorConfiguration;
        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly Notifier _notifier;
        private string _previousString;
        private readonly IEnumerable<ITranslator> _translators;

        public Finder(Notifier notifier,
            GoogleLanguageDetector languageDetector,
            GoogleAnalyticsService googleAnalytics,
            ActiveTranslatorConfiguration activeTranslatorConfiguration,
            IEnumerable<ITranslator> translators,
            ApplicationConfiguration applicationConfiguration)
        {
            _notifier = notifier;
            _languageDetector = languageDetector;
            _googleAnalytics = googleAnalytics;
            _activeTranslatorConfiguration = activeTranslatorConfiguration;
            _translators = translators;
            _applicationConfiguration = applicationConfiguration;
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
            var findFunc = _translators
                .Where(x => _activeTranslatorConfiguration.Translators.Select(x => x.Type).Contains(x.GetType()))
                .Select(x => x.Translate(new TranslateRequest(currentString, fromLanguageExtension), cancellationToken))
                .ToList();

            return Task.WhenAll(findFunc.ToArray());
        }

        private async Task Trace(string currentString, string fromLanguageExtension)
        {
            await _googleAnalytics.TrackEventAsync("DynamicTranslator",
                "Translate",
                $"{currentString} | {fromLanguageExtension} - {_applicationConfiguration.ToLanguage.Extension} | v{ApplicationVersion.GetCurrentVersion()} ",
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