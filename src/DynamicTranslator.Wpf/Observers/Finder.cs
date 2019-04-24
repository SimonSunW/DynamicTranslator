using System;
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
	public class Finder : IObserver<EventPattern<WhenClipboardContainsTextEventArgs>>
	{
		private readonly DynamicTranslatorServices _services;
		private readonly GoogleAnalyticsService _googleAnalytics;
		private readonly GoogleLanguageDetector _languageDetector;
		private readonly Notifier _notifier;
		private string _previousString;

		public Finder(Notifier notifier,
			GoogleLanguageDetector languageDetector,
			DynamicTranslatorServices services,
			GoogleAnalyticsService googleAnalytics)
		{
			_notifier = notifier;
			_languageDetector = languageDetector;
			_services = services;
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

					TranslateResult[] results = await FindMeans(currentString, fromLanguageExtension, CancellationToken.None);
					string means = await new ResultOrganizer().OrganizeResult(results, currentString, out string failedResults);
					Notify(currentString, means);

					await Trace(currentString, fromLanguageExtension);
				}
				catch (Exception ex)
				{
					Notify("Error", ex.Message);
				}
			});
		}

		private Task<TranslateResult[]> FindMeans(string currentString, string fromLanguageExtension, CancellationToken cancellationToken)
		{
			List<Task<TranslateResult>> findFunc = _services
				.ActiveTranslatorConfiguration
				.ActiveTranslators
				.Select(x => x.Find(new TranslateRequest(currentString, fromLanguageExtension), cancellationToken)).ToList();

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