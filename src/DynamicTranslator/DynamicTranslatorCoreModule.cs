using System;
using System.Collections.Generic;
using DynamicTranslator.Configuration;
using DynamicTranslator.Configuration.UniqueIdentifier;
using DynamicTranslator.Extensions;
using DynamicTranslator.Google;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.Prompt;
using DynamicTranslator.SesliSozluk;
using DynamicTranslator.Tureng;
using DynamicTranslator.Yandex;

namespace DynamicTranslator
{
	public class DynamicTranslatorCoreModule
	{
		public DynamicTranslatorCoreModule(DynamicTranslatorConfiguration configurations)
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
				ClientConfiguration = new ClientConfiguration(configurations),
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

			configurations.ApplicationConfiguration = applicationConfiguration;

			var google = new DynamicTranslatorGoogleModule(configurations);
			var yandex = new DynamicTranslatorYandexModule(configurations);
			var sesliSozluk = new DynamicTranslatorSesliSozlukModule(configurations);
			var tureng = new DynamicTranslatorTurengModule(configurations);
			var prompt = new DynamicTranslatorPromptModule(configurations);

			configurations.GoogleAnalyticsConfiguration = googleAnalytics;
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
