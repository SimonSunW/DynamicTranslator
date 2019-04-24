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
using Point = System.Drawing.Point;

namespace DynamicTranslator.Wpf
{
    public class TranslatorBootstrapper
    {
        private readonly DynamicTranslatorConfiguration _configurations;
        private readonly ClipboardManager _clipboardManager;
        private readonly GrowlNotifications _growlNotifications;
        private readonly MainWindow _mainWindow;
        private CancellationTokenSource _cancellationTokenSource;
        private IDisposable _finderObservable;
        private IKeyboardMouseEvents _globalMouseHook;
        private IntPtr _hWndNextViewer;
        private HwndSource _hWndSource;
        private Point _mouseFirstPoint;
        private Point _mouseSecondPoint;
        private IDisposable _syncObserver;
        private readonly InterlockedBoolean _isMouseDown = new InterlockedBoolean();

        public TranslatorBootstrapper(MainWindow mainWindow, GrowlNotifications growlNotifications, DynamicTranslatorConfiguration configurations, ClipboardManager clipboardManager)
        {
            _mainWindow = mainWindow;
            _growlNotifications = growlNotifications;
            _configurations = configurations;
            _clipboardManager = clipboardManager;
        }

        public event EventHandler<WhenClipboardContainsTextEventArgs> WhenClipboardContainsTextEventHandler;

        public void Dispose()
        {
            if (IsInitialized)
            {
                if (_cancellationTokenSource.Token.CanBeCanceled) _cancellationTokenSource.Cancel(false);

                DisposeHooks();
                SendKeys.Flush();
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
            Task.Run(SendKeys.Flush);
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
            _growlNotifications.Top = SystemParameters.WorkArea.Top + _configurations.ApplicationConfiguration.TopOffset;
            _growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - _configurations.ApplicationConfiguration.LeftOffset;
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
            Win32.SendMessage(_hWndNextViewer, msg, wParam, lParam); //pass the message to the next viewer //clipboard content changed

            if (!_clipboardManager.ContainsText()) return;

            string currentText = _clipboardManager.GetCurrentText();

            if (!string.IsNullOrEmpty(currentText))
            {
                TextCaptured(currentText);
                _clipboardManager.Clear();
            }
        }

        private void MouseDoubleClicked(object sender, MouseEventArgs e)
        {
            _isMouseDown.Set(false);

            if (_cancellationTokenSource.Token.IsCancellationRequested) ;

            SendCopyCommand();
        }

        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested) return;

            if ((e.Button & MouseButtons.Left) != 0)
            {
                _isMouseDown.Set(true);
                _mouseFirstPoint = e.Location;
            }
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if (_isMouseDown.Value && !(_mouseFirstPoint.X == _mouseSecondPoint.X && _mouseFirstPoint.Y == _mouseSecondPoint.Y))
                {
                    _isMouseDown.Set(false);
                    _mouseSecondPoint = e.Location;
                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                    SendCopyCommand();
                }
            }
        }

        private void StartHooks()
        {
            var wih = new WindowInteropHelper(_mainWindow);
            _hWndSource = HwndSource.FromHwnd(wih.Handle);
            _globalMouseHook = Hook.GlobalEvents();
            var source = _hWndSource;

            if (source == null) return;

            source.AddHook(WinProc); // start processing window messages
            _hWndNextViewer = Win32.SetClipboardViewer(source.Handle); // set this window as a viewer
        }

        private void StartObservers()
        {
            var finder = new Finder(
                new Notifier(_growlNotifications),
                new GoogleLanguageDetector(_configurations),
                _configurations,
                new GoogleAnalyticsService(_configurations.ApplicationConfiguration));

            var googleAnalytics = new GoogleAnalyticsTracker(new GoogleAnalyticsService(_configurations.ApplicationConfiguration));

            _finderObservable = Observable
                .FromEventPattern<WhenClipboardContainsTextEventArgs>(
                    h => WhenClipboardContainsTextEventHandler += h,
                    h => WhenClipboardContainsTextEventHandler -= h)
                .Subscribe(finder);

            _syncObserver = Observable
                .Interval(TimeSpan.FromSeconds(7.0), TaskPoolScheduler.Default)
                .StartWith(-1L)
                .Subscribe(googleAnalytics);
        }

        private void SubscribeLocalEvents()
        {
            _globalMouseHook.MouseDoubleClick += MouseDoubleClicked;
            //_globalMouseHook.MouseDown += MouseDown;
            //_globalMouseHook.MouseUp += MouseUp;
            _globalMouseHook.MouseDragStarted += MouseDragStarted;
            _globalMouseHook.MouseDragFinished += MouseDragFinished;
        }

        private void MouseDragFinished(object sender, MouseEventArgs e)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested) return;
            _isMouseDown.Set(false);
            SendCopyCommand();

        }

        private void MouseDragStarted(object sender, MouseEventArgs e)
        {
            _isMouseDown.Set(true);
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