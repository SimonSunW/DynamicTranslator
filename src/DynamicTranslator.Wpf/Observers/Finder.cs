using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Google;
using DynamicTranslator.Model;
using DynamicTranslator.Orchestrators;
using DynamicTranslator.Wpf.Notification;

namespace DynamicTranslator.Wpf.Observers
{
	public class Finder : IObserver<EventPattern<WhenClipboardContainsTextEventArgs>>
	{
		private readonly DynamicTranslatorConfiguration _configuration;
		private readonly GoogleAnalyticsService _googleAnalytics;
		private readonly GoogleLanguageDetector _languageDetector;
		private readonly INotifier _notifier;
		private string _previousString;

		public Finder(INotifier notifier,
			GoogleLanguageDetector languageDetector,
			DynamicTranslatorConfiguration configuration,
			GoogleAnalyticsService googleAnalytics)
		{
			_notifier = notifier;
			_languageDetector = languageDetector;
			_configuration = configuration;
			_googleAnalytics = googleAnalytics;
		}

		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public void OnNext(EventPattern<WhenClipboardContainsTextEventArgs> value)
		{
			Task.Run(async () =>
			{
				try
				{
					string currentString = value.EventArgs.CurrentString;

					if (_previousString == currentString) return;

					_previousString = currentString;

					string fromLanguageExtension = await _languageDetector.DetectLanguage(currentString);
					var results = new List<TranslateResult>(); //await GetMeansFromCache(currentString, fromLanguageExtension);
					var means = await new ResultOrganizer().OrganizeResult(results, currentString, out string failedResults);

					await Notify(currentString, means);
					await Notify(currentString, failedResults);
					await Trace(currentString, fromLanguageExtension);
				}
				catch (Exception ex)
				{
					await Notify("Error", ex.Message);
				}
			});
		}

		private async Task Trace(string currentString, string fromLanguageExtension)
		{
			await _googleAnalytics.TrackEventAsync("DynamicTranslator",
				"Translate",
				$"{currentString} | {fromLanguageExtension} - {_configuration.ApplicationConfiguration.ToLanguage.Extension} | v{ApplicationVersion.GetCurrentVersion()} ",
				null).ConfigureAwait(false);

			await _googleAnalytics.TrackAppScreenAsync("DynamicTranslator",
				ApplicationVersion.GetCurrentVersion(),
				"dynamictranslator",
				"dynamictranslator",
				"notification").ConfigureAwait(false);
		}

		private Task Notify(string currentString, string means)
		{
			return !string.IsNullOrEmpty(means) 
				? _notifier.AddNotificationAsync(currentString, ImageUrls.NotificationUrl, means) 
				: Task.CompletedTask;
		}
	}
}