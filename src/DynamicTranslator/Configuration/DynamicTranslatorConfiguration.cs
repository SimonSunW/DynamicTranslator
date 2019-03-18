using System;
using System.Collections.Generic;
using DynamicTranslator.Configuration.UniqueIdentifier;
using DynamicTranslator.Extensions;
using DynamicTranslator.Google;
using DynamicTranslator.Model;
using DynamicTranslator.Prompt;
using DynamicTranslator.SesliSozluk;
using DynamicTranslator.Tureng;
using DynamicTranslator.Yandex;
using RestSharp;

namespace DynamicTranslator.Configuration
{
	public class DynamicTranslatorConfiguration
	{
		public ActiveTranslatorConfiguration ActiveTranslatorConfiguration { get; set; } = new ActiveTranslatorConfiguration();

		public AppConfigManager AppConfigManager { get; set; } = new AppConfigManager();

		public Func<IRestClient> ClientFactory => () => new RestClient();

		public ApplicationConfiguration ApplicationConfiguration { get; set; } = new ApplicationConfiguration();

		public GoogleAnalyticsConfiguration GoogleAnalyticsConfiguration { get; set; }

		public GoogleTranslatorConfiguration GoogleTranslatorConfiguration { get; set; }

		public PromptTranslatorConfiguration PromptTranslatorConfiguration { get; set; }

		public TurengTranslatorConfiguration TurengTranslatorConfiguration { get; set; }

		public SesliSozlukTranslatorConfiguration SesliSozlukTranslatorConfiguration { get; set; }

		public YandexTranslatorConfiguration YandexTranslatorConfiguration { get; set; }

		public void Configure()
		{
			var appConfigManager = new AppConfigManager();
			string existingToLanguage = appConfigManager.Get("ToLanguage");
			string existingFromLanguage = appConfigManager.Get("FromLanguage");
			var applicationConfiguration = new ApplicationConfiguration
			{
				IsLanguageDetectionEnabled = true,
				IsExtraLoggingEnabled = true,
				LeftOffset = 500,
				TopOffset = 15,
				SearchableCharacterLimit = 200,
				IsNoSqlDatabaseEnabled = true,
				MaxNotifications = 4,
				ToLanguage = new Language(existingToLanguage, LanguageMapping.All[existingToLanguage]),
				FromLanguage = new Language(existingFromLanguage, LanguageMapping.All[existingFromLanguage]),
				ClientConfiguration = new ClientConfiguration(this),
			};

			var googleAnalytics = new GoogleAnalyticsConfiguration
			{
				Url = "http://www.google-analytics.com/collect",
				TrackingId = "UA-70082243-2"
			};

			ClientConfiguration client = applicationConfiguration.ClientConfiguration;

			client.AppVersion = ApplicationVersion.GetCurrentVersion();
			client.Id = string.IsNullOrEmpty(appConfigManager.Get("ClientId")) ? GenerateUniqueClientId() : appConfigManager.Get("ClientId");
			client.MachineName = Environment.MachineName.Normalize();
			appConfigManager.SaveOrUpdate("ClientId", client.Id);

			ApplicationConfiguration = applicationConfiguration;

			var google = new GoogleTranslator(this);
			var yandex = new YandexTranslator(this);
			var sesliSozluk = new SesliSozlukTranslator(this);
			var tureng = new TurengTranslator(this);
			var prompt = new PromptTranslator(this);

			GoogleAnalyticsConfiguration = googleAnalytics;
			applicationConfiguration.ClientConfiguration = client;
		}

		private string GenerateUniqueClientId()
		{
			string uniqueId;
			try
			{
				var uniqueIdProviders = new List<IUniqueIdentifierProvider>()
				{
					new CpuBasedIdentifierProvider(),
					new HddBasedIdentifierProvider()
				};

				uniqueId = uniqueIdProviders.BuildForAll();
			}
			catch (Exception)
			{
				uniqueId = Guid.NewGuid().ToString();
			}

			return uniqueId;
		}
	}
}
