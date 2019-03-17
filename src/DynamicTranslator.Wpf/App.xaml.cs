using System;
using System.Security.AccessControl;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DynamicTranslator.Configuration;
using DynamicTranslator.Wpf.ViewModel;

namespace DynamicTranslator.Wpf
{
	public partial class App
	{
		private Mutex _mutex;
		private const string MutexName = @"Global\1109F104-B4B4-4ED1-920C-F4D8EFE9E834}";
		private bool _isMutexCreated;
		private bool _isMutexUnauthorized;

		public App()
		{
			CheckApplicationInstanceExist();

			var configurations = new DynamicTranslatorConfiguration();
			var dynamicTranslatorCoreModule = new DynamicTranslatorCoreModule(configurations);
			var mainWindow = new MainWindow()
			{
				Configurations = configurations
			};

			mainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
		}

		protected override void OnStartup(StartupEventArgs eventArgs)
		{
		}

		private void CheckApplicationInstanceExist()
		{
			string user = Environment.UserDomainName + "\\" + Environment.UserName;

			try
			{
				Mutex.TryOpenExisting(MutexName, out _mutex);

				if (_mutex == null)
				{
					var mutexSecurity = new MutexSecurity();

					var rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Deny);

					mutexSecurity.AddAccessRule(rule);

					rule = new MutexAccessRule(user, MutexRights.ReadPermissions | MutexRights.ChangePermissions, AccessControlType.Allow);

					mutexSecurity.AddAccessRule(rule);

					_mutex = new Mutex(true, MutexName, out _isMutexCreated, mutexSecurity);
				}
			}
			catch (UnauthorizedAccessException)
			{
				_isMutexUnauthorized = true;
			}

			if (!_isMutexUnauthorized && _isMutexCreated)
			{
				_mutex?.WaitOne();
				GC.KeepAlive(_mutex);
				return;
			}

			_mutex?.ReleaseMutex();
			_mutex?.Dispose();
			Current.Shutdown();
		}

		private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = true;
		}
	}
}
