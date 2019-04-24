using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DynamicTranslator.Configuration.UniqueIdentifier;
using DynamicTranslator.Extensions;
using DynamicTranslator.Google;
using DynamicTranslator.Model;
using DynamicTranslator.Prompt;
using DynamicTranslator.SesliSozluk;
using DynamicTranslator.Tureng;
using DynamicTranslator.Yandex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicTranslator.Configuration
{
    public class DynamicTranslatorServices
    {
        public ActiveTranslatorConfiguration ActiveTranslatorConfiguration { get; set; } = new ActiveTranslatorConfiguration();

        public AppConfigManager AppConfigManager { get; private set; }

        public HttpMessageHandler MessageHandler { get; set; } = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };

        public Func<TranslatorClient> ClientFactory => () => ServiceProvider.GetService<TranslatorClient>();

        public ApplicationConfiguration ApplicationConfiguration { get; set; }

        public GoogleAnalyticsConfiguration GoogleAnalyticsConfiguration { get; set; }

        public GoogleTranslatorConfiguration GoogleTranslatorConfiguration { get; set; }

        public PromptTranslatorConfiguration PromptTranslatorConfiguration { get; set; }

        public TurengTranslatorConfiguration TurengTranslatorConfiguration { get; set; }

        public SesliSozlukTranslatorConfiguration SesliSozlukTranslatorConfiguration { get; set; }

        public YandexTranslatorConfiguration YandexTranslatorConfiguration { get; set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public DynamicTranslatorServices(Action<IConfigurationBuilder> configure = null)
        {
            var services = new ServiceCollection();
            services
                .AddHttpClient<TranslatorClient>()
                .ConfigurePrimaryHttpMessageHandler(sp => MessageHandler);

            IConfigurationBuilder cb = new ConfigurationBuilder()
                .AddIniFile("DynamicTranslator.ini", configure != null, false);

            configure?.Invoke(cb);

            IConfigurationRoot configuration = cb.Build();

            AppConfigManager = new AppConfigManager(configuration);
            string existingToLanguage = configuration["ToLanguage"];
            string existingFromLanguage = configuration["FromLanguage"];
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
            client.Id = string.IsNullOrEmpty(AppConfigManager.Get("ClientId")) ? GenerateUniqueClientId() : AppConfigManager.Get("ClientId");
            client.MachineName = Environment.MachineName.Normalize();
            AppConfigManager.SaveOrUpdate("ClientId", client.Id);

            ApplicationConfiguration = applicationConfiguration;

            var google = new GoogleTranslator(this);
            var yandex = new YandexTranslator(this);
            var sesliSozluk = new SesliSozlukTranslator(this);
            var tureng = new TurengTranslator(this);
            var prompt = new PromptTranslator(this);

            GoogleAnalyticsConfiguration = googleAnalytics;
            applicationConfiguration.ClientConfiguration = client;

            ServiceProvider = services.BuildServiceProvider();
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

        public async Task<string> FindBy(string text, TranslatorType translatorType)
        {
            Translator translator = ActiveTranslatorConfiguration
                .ActiveTranslators
                .FirstOrDefault(x => x.Type == translatorType && x.IsActive);

            if (translator == null)
            {
                return "Translator is not active";
            }

            var result = await translator.Find(new TranslateRequest(text, ApplicationConfiguration.FromLanguage.Extension));
            return result.IsSuccess ? result.ResultMessage : "Error during the translation";
        }
    }
}
