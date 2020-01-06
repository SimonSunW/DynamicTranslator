using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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
        private readonly WireUp _services;
        private readonly ClipboardManager _clipboardManager;
        private readonly GrowlNotifications _growlNotifications;
        private readonly MainWindow _mainWindow;
        private CancellationTokenSource _cancellationTokenSource;
        private IDisposable _finderObservable;
        private readonly IKeyboardMouseEvents _globalMouseHook;
        private Point _mouseFirstPoint;
        private Point _mouseSecondPoint;
        private IDisposable _syncObserver;
        private readonly InterlockedBoolean _isMouseDown = new InterlockedBoolean();

        public TranslatorBootstrapper(MainWindow mainWindow, GrowlNotifications growlNotifications, WireUp services, ClipboardManager clipboardManager)
        {
            _mainWindow = mainWindow;
            _growlNotifications = growlNotifications;
            _services = services;
            _clipboardManager = clipboardManager;
            _globalMouseHook = Hook.GlobalEvents();
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

            if (!_clipboardManager.ContainsText()) return;

            var currentText = _clipboardManager.GetCurrentText();

            if (!string.IsNullOrEmpty(currentText))
            {
                TextCaptured(currentText);
                _clipboardManager.Clear();
            }
        }

        private void ConfigureNotificationMeasurements()
        {
            _growlNotifications.Top = SystemParameters.WorkArea.Top + _services.ApplicationConfiguration.TopOffset;
            _growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - _services.ApplicationConfiguration.LeftOffset;
        }

        private void DisposeHooks()
        {
            _globalMouseHook.Dispose();
        }
        
        private void MouseDoubleClicked(object sender, MouseEventArgs e)
        {
            _isMouseDown.Set(false);

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

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

        private void StartObservers()
        {
            var finder = new Finder(
                new Notifier(_growlNotifications),
                new GoogleLanguageDetector(),
                _services,
                new GoogleAnalyticsService(_services.ApplicationConfiguration));

            var googleAnalytics = new GoogleAnalyticsTracker(new GoogleAnalyticsService(_services.ApplicationConfiguration));

            _finderObservable = Observable
                .FromEventPattern<WhenClipboardContainsTextEventArgs>(
                    h => WhenClipboardContainsTextEventHandler += h,
                    h => WhenClipboardContainsTextEventHandler -= h)
                .Select(pattern => Observable.FromAsync(token => finder.Find(pattern, token)))
                .Concat()
                .Subscribe();

            _syncObserver = Observable
                .Interval(TimeSpan.FromSeconds(7.0), TaskPoolScheduler.Default)
                .StartWith(-1L)
                .Select((l, i) => googleAnalytics.Track())
                .Subscribe();
        }

        private void SubscribeLocalEvents()
        {
            _globalMouseHook.MouseDoubleClick += MouseDoubleClicked;
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
    }
}