using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DynamicTranslator.Configuration;
using DynamicTranslator.Model;
using DynamicTranslator.Wpf.Extensions;

using Octokit;

using Language = DynamicTranslator.Model.Language;

namespace DynamicTranslator.Wpf.ViewModel
{
	public partial class MainWindow
	{
		public DynamicTranslatorServices Serviceses { get; set; }
		public TranslatorBootstrapper Translator { get; private set; }
		private GitHubClient _gitHubClient;
		private Func<string, bool> _isNewVersion;
		private bool _isRunning;

		protected override void OnClosing(CancelEventArgs e)
		{
			Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
			base.OnClosing(e);
		}

		protected override void OnInitialized(EventArgs e)
		{
			InitializeComponent();
			base.OnInitialized(e);

			Translator = new TranslatorBootstrapper(
				this,
				new GrowlNotifications(Serviceses.ApplicationConfiguration, new Notifications()),
				Serviceses,
				new ClipboardManager());

			Translator.SubscribeShutdownEvents();
			_gitHubClient = new GitHubClient(new ProductHeaderValue(Serviceses.ApplicationConfiguration.GitHubRepositoryName));
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

				Translator.Dispose();
			}
			else
			{
				BtnSwitch.Content = "Stop Translator";

				string selectedLanguageName = ((Language)ComboBoxLanguages.SelectedItem).Name;
				Serviceses.AppConfigManager.SaveOrUpdate("ToLanguage", selectedLanguageName);
				Serviceses.ApplicationConfiguration.ToLanguage = new Language(selectedLanguageName, LanguageMapping.All[selectedLanguageName]);

				PrepareTranslators();
				LockUiElements();

				this.DispatchingAsync(() =>
				{
					if (!Translator.IsInitialized)
					{
						Translator.Initialize();
					}
				});

				_isRunning = true;
			}
		}

		private void FillLanguageCombobox()
		{
			foreach (Language language in LanguageMapping.All.ToLanguages())
			{
				ComboBoxLanguages.Items.Add(language);
			}

			ComboBoxLanguages.SelectedValue = Serviceses.ApplicationConfiguration.ToLanguage.Extension;
		}

		private Task<Release> GetRelease()
		{
			return _gitHubClient.Repository.Release.GetLatest(Serviceses.ApplicationConfiguration.GitHubRepositoryOwnerName, Serviceses.ApplicationConfiguration.GitHubRepositoryName);
		}

		private void GithubButtonClick(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/DynamicTranslator/DynamicTranslator");
		}

		private void InitializeVersionChecker()
		{
			NewVersionButton.Visibility = Visibility.Hidden;
			CheckVersion();
		}

		private void CheckVersion()
		{
			Release release = GetRelease().Result;

			string incomingVersion = release.TagName;

			if (_isNewVersion(incomingVersion))
			{
				this.DispatchingAsync(() =>
				{
					NewVersionButton.Visibility = Visibility.Visible;
					NewVersionButton.Content = $"A new version {incomingVersion} released, update now!";
					Serviceses.ApplicationConfiguration.UpdateLink = release.Assets.FirstOrDefault()?.BrowserDownloadUrl;
				});
			}
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
			string updateLink = Serviceses.ApplicationConfiguration.UpdateLink;
			if (!string.IsNullOrEmpty(updateLink))
			{
				Process.Start(updateLink);
			}
		}

		private void PrepareTranslators()
		{
			Serviceses.ActiveTranslatorConfiguration.DeActivate();

			if (CheckBoxGoogleTranslate.IsChecked != null && CheckBoxGoogleTranslate.IsChecked.Value)
			{
				Serviceses.ActiveTranslatorConfiguration.Activate(TranslatorType.Google);
			}

			if (CheckBoxYandexTranslate.IsChecked != null && CheckBoxYandexTranslate.IsChecked.Value)
			{
				Serviceses.ActiveTranslatorConfiguration.Activate(TranslatorType.Yandex);
			}

			if (CheckBoxTureng.IsChecked != null && CheckBoxTureng.IsChecked.Value)
			{
				Serviceses.ActiveTranslatorConfiguration.Activate(TranslatorType.Tureng);
			}

			if (CheckBoxSesliSozluk.IsChecked != null && CheckBoxSesliSozluk.IsChecked.Value)
			{
				Serviceses.ActiveTranslatorConfiguration.Activate(TranslatorType.SesliSozluk);
			}

			if (CheckBoxPrompt.IsChecked != null && CheckBoxPrompt.IsChecked.Value)
			{
				Serviceses.ActiveTranslatorConfiguration.Activate(TranslatorType.Prompt);
			}

			if (!Serviceses.ActiveTranslatorConfiguration.ActiveTranslators.Any())
			{
				foreach (object value in Enum.GetValues(typeof(TranslatorType)))
				{
					Serviceses.ActiveTranslatorConfiguration.Activate((TranslatorType)value);
				}
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
