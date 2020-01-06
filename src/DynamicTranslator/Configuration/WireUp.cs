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
    public class WireUp
    {
        public ActiveTranslatorConfiguration ActiveTranslatorConfiguration { get; set; } = new ActiveTranslatorConfiguration();

        public HttpMessageHandler MessageHandler { get; set; } = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };

        public Func<TranslatorClient> ClientFactory => () => ServiceProvider.GetService<TranslatorClient>();

        public ApplicationConfiguration ApplicationConfiguration { get; }


        public IServiceProvider ServiceProvider { get; private set; }

        private readonly List<TranslatorConfiguration> _translatorConfigurations = new List<TranslatorConfiguration>();

        public WireUp(Action<IConfigurationBuilder> configure = null)
        {
            var cb = new ConfigurationBuilder()
                .AddIniFile("DynamicTranslator.ini", configure != null, false);
            configure?.Invoke(cb);
            var configuration = cb.Build();
            
            var services = new ServiceCollection();
            services
                .AddGoogleTranslator(google => { google.SupportedLanguages = LanguageMapping.All.ToLanguages(); })
                .AddYandexTranslator(yandex => { yandex.SupportedLanguages = LanguageMapping.Yandex.ToLanguages(); })
                .AddPromptTranslator(prompt => { prompt.SupportedLanguages = LanguageMapping.Prompt.ToLanguages(); })
                .AddSesliSozlukTranslator(sesliSozluk => { sesliSozluk.SupportedLanguages = LanguageMapping.SesliSozluk.ToLanguages(); })
                .AddTurengTranslator(tureng => { tureng.SupportedLanguages = LanguageMapping.Tureng.ToLanguages(); })
                .AddSingleton(configuration)
                .AddSingleton(sp =>
                {
                    var appConfigManager = sp.GetService<AppConfigManager>();
                    var clientConfiguration = new ClientConfiguration
                    {
                        AppVersion = ApplicationVersion.GetCurrentVersion(),
                        Id = string.IsNullOrEmpty(appConfigManager.Get("ClientId")) ? GenerateUniqueClientId() : appConfigManager.Get("ClientId"),
                        MachineName = Environment.MachineName.Normalize(),
                    };
                    var existingToLanguage = configuration["ToLanguage"];
                    var existingFromLanguage = configuration["FromLanguage"];
                    appConfigManager.SaveOrUpdate("ClientId", clientConfiguration.Id);

                    return new ApplicationConfiguration
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
                        ClientConfiguration = clientConfiguration
                    };
                })
                .AddSingleton<AppConfigManager>()
                .AddHttpClient<TranslatorClient>()
                .ConfigurePrimaryHttpMessageHandler(sp => MessageHandler);
            
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
            var translator = ActiveTranslatorConfiguration
                .ActiveTranslators
                .FirstOrDefault(x => x.Type == translatorType && x.IsActive);

            if (translator == null)
            {
                return "Translator is not active";
            }

            var result = await translator.Find(new TranslateRequest(text, ApplicationConfiguration.FromLanguage.Extension));
            return result.IsSuccess ? result.ResultMessage : "Error during the translation";
        }

        public void AddTranslatorConfiguration(TranslatorConfiguration configuration)
        {
            _translatorConfigurations.Add(configuration);
        }
    }

    public static class DetectorExtensions
    {
        public static IServiceCollection AddGoogleTranslator(this IServiceCollection services, Action<GoogleTranslatorConfiguration> configure)
        {
            services.AddSingleton(sp =>
            {
                var configuration = new GoogleTranslatorConfiguration(
                    sp.GetService<ActiveTranslatorConfiguration>(),
                    sp.GetService<ApplicationConfiguration>());

                configure(configuration);
                return configuration;
            });

            return services;
        }

        public static IServiceCollection AddYandexTranslator(this IServiceCollection services, Action<YandexTranslatorConfiguration> configure)
        {
            services.AddSingleton(sp =>
            {
                var configuration = new YandexTranslatorConfiguration(
                    sp.GetService<ActiveTranslatorConfiguration>(),
                    sp.GetService<ApplicationConfiguration>());

                configure(configuration);
                return configuration;
            });

            return services;
        }

        public static IServiceCollection AddSesliSozlukTranslator(this IServiceCollection services, Action<SesliSozlukTranslatorConfiguration> configure)
        {
            services.AddSingleton(sp =>
            {
                var configuration = new SesliSozlukTranslatorConfiguration(
                    sp.GetService<ActiveTranslatorConfiguration>(),
                    sp.GetService<ApplicationConfiguration>());

                configure(configuration);
                return configuration;
            });

            return services;
        }

        public static IServiceCollection AddTurengTranslator(this IServiceCollection services, Action<TurengTranslatorConfiguration> configure)
        {
            services.AddSingleton(sp =>
            {
                var configuration = new TurengTranslatorConfiguration(
                    sp.GetService<ActiveTranslatorConfiguration>(),
                    sp.GetService<ApplicationConfiguration>());

                configure(configuration);
                return configuration;
            });

            return services;
        }


        public static IServiceCollection AddPromptTranslator(this IServiceCollection services, Action<PromptTranslatorConfiguration> configure)
        {
            services.AddSingleton(sp =>
            {
                var configuration = new PromptTranslatorConfiguration(
                    sp.GetService<ActiveTranslatorConfiguration>(),
                    sp.GetService<ApplicationConfiguration>());

                configure(configuration);
                return configuration;
            });

            return services;
        }

    }
}
