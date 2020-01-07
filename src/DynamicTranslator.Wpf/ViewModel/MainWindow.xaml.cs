using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DynamicTranslator.Configuration;
using DynamicTranslator.Google;
using DynamicTranslator.Model;
using DynamicTranslator.Prompt;
using DynamicTranslator.SesliSozluk;
using DynamicTranslator.Tureng;
using DynamicTranslator.Wpf.Extensions;
using DynamicTranslator.Wpf.Observers;
using DynamicTranslator.Yandex;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Language = DynamicTranslator.Model.Language;

namespace DynamicTranslator.Wpf.ViewModel
{
    public partial class MainWindow : Window
    {
        private ActiveTranslatorConfiguration _activeTranslatorConfiguration;
        private ApplicationConfiguration _applicationConfiguration;
        private GitHubClient _gitHubClient;
        private Func<string, bool> _isNewVersion;
        private bool _isRunning;
        public IServiceProvider ServiceProvider { get; }
        public TranslatorBootstrapper Translator { get; private set; }

        public MainWindow()
        {
            var wireUp = new WireUp(postConfigureServices: services =>
            {
                services.AddTransient<IFinder, Finder>();
                services.AddSingleton<Notifications>();
                services.AddTransient<ClipboardManager>();
                services.AddSingleton<GrowlNotifications>();
                services.AddTransient<TranslatorBootstrapper>();
                services.AddTransient<Notifier>();
            });
            ServiceProvider = wireUp.ServiceProvider;
        }
        protected override void OnInitialized(EventArgs e)
        {
            InitializeComponent();
            base.OnInitialized(e);

            _applicationConfiguration = ServiceProvider.GetService<ApplicationConfiguration>();
            _activeTranslatorConfiguration = ServiceProvider.GetService<ActiveTranslatorConfiguration>();
            Translator = ServiceProvider.GetService<TranslatorBootstrapper>();

            _gitHubClient = new GitHubClient(new ProductHeaderValue("DynamicTranslator"));
            _isNewVersion = version =>
            {
                var currentVersion = new Version(ApplicationVersion.GetCurrentVersion());
                var newVersion = new Version(version);

                return newVersion > currentVersion;
            };

            FillLanguageCombobox();
            //InitializeVersionChecker();
        }

        private void BtnSwitchClick(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                BtnSwitch.Content = "Start Translator";

                _isRunning = false;

                UnlockUiElements();
            }
            else
            {
                BtnSwitch.Content = "Stop Translator";

                var selectedLanguageName = ((Language)ComboBoxLanguages.SelectedItem).Name;
                _applicationConfiguration.ToLanguage =
                    new Language(selectedLanguageName, LanguageMapping.All[selectedLanguageName]);

                PrepareTranslators();
                LockUiElements();

                this.DispatchingAsync(() =>
                {
                    if (!Translator.IsInitialized) Translator.Initialize();
                });

                _isRunning = true;
            }
        }

        private void FillLanguageCombobox()
        {
            foreach (var language in LanguageMapping.All.ToLanguages()) ComboBoxLanguages.Items.Add(language);

            ComboBoxLanguages.SelectedValue = _applicationConfiguration.ToLanguage.Extension;
        }

        private Task<Release> GetRelease()
        {
            return _gitHubClient.Repository.Release.GetLatest("DynamicTranslator", "DynamicTranslator");
        }

        private void InitializeVersionChecker()
        {
            //NewVersionButton.Visibility = Visibility.Hidden;
            CheckVersion();
        }

        private void CheckVersion()
        {
            var release = GetRelease().Result;

            var incomingVersion = release.TagName;

            if (_isNewVersion(incomingVersion))
                this.DispatchingAsync(() =>
                {
                    //NewVersionButton.Visibility = Visibility.Visible;
                    //NewVersionButton.Content = $"A new version {incomingVersion} released, update now!";
                    _applicationConfiguration.UpdateLink = release.Assets.FirstOrDefault()?.BrowserDownloadUrl;
                });
        }

        private void LockUiElements()
        {
            this.DispatchingAsync(() =>
            {
                ComboBoxLanguages.Focusable = false;
                ComboBoxLanguages.IsHitTestVisible = false;
                CheckBoxGoogleTranslate.IsHitTestVisible = false;
                CheckBoxTureng.IsHitTestVisible = false;
                CheckBoxYandexTranslate.IsHitTestVisible = false;
                CheckBoxSesliSozluk.IsHitTestVisible = false;
                CheckBoxPrompt.IsHitTestVisible = false;
            });
        }

        private void NewVersionButtonClick(object sender, RoutedEventArgs e)
        {
            var updateLink = _applicationConfiguration.UpdateLink;
            if (!string.IsNullOrEmpty(updateLink)) Process.Start(updateLink);
        }

        private void PrepareTranslators()
        {
            _activeTranslatorConfiguration.DeActivate();

            if (CheckBoxGoogleTranslate.IsChecked != null && CheckBoxGoogleTranslate.IsChecked.Value)
            {
                _activeTranslatorConfiguration.AddTranslator<GoogleTranslator>();
                _activeTranslatorConfiguration.Activate<GoogleTranslator>();
            }

            if (CheckBoxYandexTranslate.IsChecked != null && CheckBoxYandexTranslate.IsChecked.Value)
            {
                _activeTranslatorConfiguration.AddTranslator<YandexTranslator>();
                _activeTranslatorConfiguration.Activate<YandexTranslator>();
            }

            if (CheckBoxTureng.IsChecked != null && CheckBoxTureng.IsChecked.Value)
            {
                _activeTranslatorConfiguration.AddTranslator<TurengTranslator>();
                _activeTranslatorConfiguration.Activate<TurengTranslator>();
            }

            if (CheckBoxSesliSozluk.IsChecked != null && CheckBoxSesliSozluk.IsChecked.Value)
            {
                _activeTranslatorConfiguration.AddTranslator<SesliSozlukTranslator>();
                _activeTranslatorConfiguration.Activate<SesliSozlukTranslator>();
            }

            if (CheckBoxPrompt.IsChecked != null && CheckBoxPrompt.IsChecked.Value)
            {
                _activeTranslatorConfiguration.AddTranslator<PromptTranslator>();
                _activeTranslatorConfiguration.Activate<PromptTranslator>();
            }

            if (!_activeTranslatorConfiguration.ActiveTranslators.Any())
            {
                _activeTranslatorConfiguration.Activate<GoogleTranslator>();
                _activeTranslatorConfiguration.Activate<YandexTranslator>();
                _activeTranslatorConfiguration.Activate<TurengTranslator>();
                _activeTranslatorConfiguration.Activate<PromptTranslator>();
                _activeTranslatorConfiguration.Activate<SesliSozlukTranslator>();
            }

        }

        private void UnlockUiElements()
        {
            ComboBoxLanguages.Focusable = true;
            ComboBoxLanguages.IsHitTestVisible = true;
            CheckBoxGoogleTranslate.IsHitTestVisible = true;
            CheckBoxTureng.IsHitTestVisible = true;
            CheckBoxYandexTranslate.IsHitTestVisible = true;
            CheckBoxSesliSozluk.IsHitTestVisible = true;
            CheckBoxPrompt.IsHitTestVisible = true;
        }
    }
}