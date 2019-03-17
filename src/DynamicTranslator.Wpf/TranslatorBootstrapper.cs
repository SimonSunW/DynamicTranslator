using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using DynamicTranslator.Wpf.Observers;
using DynamicTranslator.Wpf.ViewModel;
using Gma.System.MouseKeyHook;
using System.Reactive.Concurrency;
using DynamicTranslator.Configuration;
using DynamicTranslator.Google;
using DynamicTranslator.Wpf.Extensions;
using Point = System.Drawing.Point;

namespace DynamicTranslator.Wpf
{
	public class TranslatorBootstrapper
	{
		private readonly DynamicTranslatorConfiguration _dynamicTranslatorConfiguration;
		private readonly ClipboardManager _clipboardManager;
		private readonly GrowlNotifications _growlNotifications;
		private readonly MainWindow _mainWindow;
		private CancellationTokenSource _cancellationTokenSource;
		private IDisposable _finderObservable;
		private IKeyboardMouseEvents _globalMouseHook;
		private IntPtr _hWndNextViewer;
		private HwndSource _hWndSource;
		private volatile bool _isMouseDown;
		private Point _mouseFirstPoint;
		private Point _mouseSecondPoint;
		private IDisposable _syncObserver;

		public TranslatorBootstrapper(MainWindow mainWindow,
			GrowlNotifications growlNotifications,
			DynamicTranslatorConfiguration dynamicTranslatorConfiguration,
			ClipboardManager clipboardManager)
		{
			_mainWindow = mainWindow;
			_growlNotifications = growlNotifications;
			_dynamicTranslatorConfiguration = dynamicTranslatorConfiguration;
			_clipboardManager = clipboardManager;
		}

		public event EventHandler<WhenClipboardContainsTextEventArgs> WhenClipboardContainsTextEventHandler;

		public void Dispose()
		{
			if (IsInitialized)
			{
				if (_cancellationTokenSource.Token.CanBeCanceled) _cancellationTokenSource.Cancel(false);

				DisposeHooks();
				Task.Run(() => SendKeys.Flush());
				UnsubscribeLocalEvents();
				_growlNotifications.Dispose();
				_finderObservable.Dispose();
				_syncObserver.Dispose();
				IsInitialized = false;
			}
		}

		public void Initialize()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			StartHooks();
			ConfigureNotificationMeasurements();
			SubscribeLocalEvents();
			Task.Run(() => SendKeys.Flush());
			StartObservers();
			IsInitialized = true;
		}

		public void SubscribeShutdownEvents()
		{
			_mainWindow.Dispatcher.ShutdownStarted += (sender, args) => { _cancellationTokenSource?.Cancel(false); };
			_mainWindow.Dispatcher.ShutdownFinished += (sender, args) => { Dispose(); };
		}

		public bool IsInitialized { get; private set; }

		private void SendCopyCommand()
		{
			SendKeys.SendWait("^c");
			SendKeys.Flush();
		}

		private void ConfigureNotificationMeasurements()
		{
			_growlNotifications.Top = SystemParameters.WorkArea.Top + _dynamicTranslatorConfiguration.ApplicationConfiguration.TopOffset;
			_growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - _dynamicTranslatorConfiguration.ApplicationConfiguration.LeftOffset;
		}

		private void DisposeHooks()
		{
			Win32.ChangeClipboardChain(_hWndSource.Handle, _hWndNextViewer);
			_hWndNextViewer = IntPtr.Zero;
			_hWndSource.RemoveHook(WinProc);
			_globalMouseHook.Dispose();
		}

		private void HandleTextCaptured(int msg, IntPtr wParam, IntPtr lParam)
		{
			_mainWindow.DispatchingAsync(() =>
			{
				Win32.SendMessage(_hWndNextViewer, msg, wParam, lParam); //pass the message to the next viewer //clipboard content changed

				if (!_clipboardManager.ContainsText()) return;

				string currentText = _clipboardManager.GetCurrentText();

				if (!string.IsNullOrEmpty(currentText))
				{
					TextCaptured(currentText);
					_clipboardManager.Clear();
				}
			});
		}

		private void MouseDoubleClicked(object sender, MouseEventArgs e)
		{
			_isMouseDown = false;
			if (_cancellationTokenSource.Token.IsCancellationRequested) ;

			SendCopyCommand();
		}

		private void MouseDown(object sender, MouseEventArgs e)
		{
			if (_cancellationTokenSource.Token.IsCancellationRequested) return;

			_mouseFirstPoint = e.Location;
			_isMouseDown = true;
		}

		private void MouseUp(object sender, MouseEventArgs e)
		{
			if (_isMouseDown && !_mouseSecondPoint.Equals(_mouseFirstPoint))
			{
				_mouseSecondPoint = e.Location;
				if (_cancellationTokenSource.Token.IsCancellationRequested) return;

				SendCopyCommand();
				_isMouseDown = false;
			}
		}

		private void StartHooks()
		{
			var wih = new WindowInteropHelper(_mainWindow);
			_hWndSource = HwndSource.FromHwnd(wih.Handle);
			_globalMouseHook = Hook.GlobalEvents();
			var source = _hWndSource;
			if (source != null)
			{
				source.AddHook(WinProc); // start processing window messages
				_hWndNextViewer = Win32.SetClipboardViewer(source.Handle); // set this window as a viewer
			}
		}

		private void StartObservers()
		{
			_finderObservable = Observable
				.FromEventPattern<WhenClipboardContainsTextEventArgs>(
					h => WhenClipboardContainsTextEventHandler += h,
					h => WhenClipboardContainsTextEventHandler -= h).Subscribe(
					new Finder(
						new Notifier(_growlNotifications),
						new GoogleLanguageDetector(_dynamicTranslatorConfiguration),
						_dynamicTranslatorConfiguration,
						new GoogleAnalyticsService(_dynamicTranslatorConfiguration.ApplicationConfiguration)));

			_syncObserver = Observable
				.Interval(TimeSpan.FromSeconds(7.0), TaskPoolScheduler.Default)
				.StartWith(-1L)
				.Subscribe(new GoogleAnalyticsTracker(new GoogleAnalyticsService(_dynamicTranslatorConfiguration.ApplicationConfiguration)));
		}

		private void SubscribeLocalEvents()
		{
			_globalMouseHook.MouseDoubleClick += MouseDoubleClicked;
			_globalMouseHook.MouseDown += MouseDown;
			_globalMouseHook.MouseUp += MouseUp;
		}

		private void TextCaptured(string currentText)
		{
			if (_cancellationTokenSource.Token.IsCancellationRequested) return;

			WhenClipboardContainsTextEventHandler?.Invoke(this,
			   new WhenClipboardContainsTextEventArgs { CurrentString = currentText }
		   );
		}

		private void UnsubscribeLocalEvents()
		{
			_globalMouseHook.MouseDoubleClick -= MouseDoubleClicked;
			_globalMouseHook.MouseDownExt -= MouseDown;
			_globalMouseHook.MouseUp -= MouseUp;
		}

		private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case Win32.WmChangecbchain:
					if (wParam == _hWndNextViewer)
						_hWndNextViewer = lParam; //clipboard viewer chain changed, need to fix it.
					else if (_hWndNextViewer != IntPtr.Zero)
						Win32.SendMessage(_hWndNextViewer, msg, wParam, lParam); //pass the message to the next viewer.
					break;
				case Win32.WmDrawclipboard:
					HandleTextCaptured(msg, wParam, lParam);
					break;
			}

			return IntPtr.Zero;
		}
	}
}