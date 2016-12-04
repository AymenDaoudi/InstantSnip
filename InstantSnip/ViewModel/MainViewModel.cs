using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using InstantSnip.Helpers;
using InstantSnip.Views;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;

namespace InstantSnip.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Properties

        private string _mainActionIcon;
        public string MainActionIcon
        {
            get { return _mainActionIcon; }
            set
            {
                if (_mainActionIcon == value) return;
                _mainActionIcon = value;
                RaisePropertyChanged(nameof(MainActionIcon));
            }
        }

        private WindowState _windowState;
        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                if (_windowState == value) return;
                _windowState = value;
                RaisePropertyChanged(nameof(WindowState));
            }
        }

        private ScreeShotView _screenShotViewWindow;
        public ScreeShotView ScreenShotViewWindow
        {
            get { return _screenShotViewWindow; }
            set
            {
                if (_screenShotViewWindow == value) return;
                _screenShotViewWindow = value;
                RaisePropertyChanged(nameof(ScreenShotViewWindow));
            }
        }
        #endregion

        #region RelayCommands

        private RelayCommand _closeApplication;
        public RelayCommand CloseApplication => _closeApplication ?? (_closeApplication = new RelayCommand(() => Application.Current.Shutdown()));

        private RelayCommand _startSnipping;
        public RelayCommand StartSnipping => _startSnipping ?? (_startSnipping = new RelayCommand(() =>
                                                                                                       {
                                                                                                           ViewsAccessibility.GetCorresponingWindow(this).Hide();
                                                                                                           var bitmap = CaptureSnipping();
                                                                                                           ScreenShotViewWindow = new ScreeShotView();
                                                                                                           ScreenShotViewWindow.Show();
                                                                                                           Messenger.Default.Send(bitmap);
                                                                                                       }));

        private RelayCommand _restartSnipping;
        public RelayCommand RestartSnipping => _restartSnipping ?? (_restartSnipping = new RelayCommand(() =>
                                                                                                            {
                                                                                                                ScreenShotViewWindow?.Close();
                                                                                                                SetMainAction(StartSnipping, StartSnippingIconPathData);
                                                                                                            }));

        private RelayCommand _saveSnippet;
        public RelayCommand SaveSnipping => _saveSnippet ?? (_saveSnippet = new RelayCommand(() =>
        {
            Messenger.Default.Send(SnippingState.Saved);
            SetMainAction(StartSnipping, StartSnippingIconPathData);
        }));

        private RelayCommand _mainAction;
        public RelayCommand MainAction
        {
            get
            {
                return _mainAction;
            }
            private set
            {
                if (_mainAction == value) return;
                _mainAction = value;
                RaisePropertyChanged(nameof(MainAction));
            }
        }

        private RelayCommand<System.Windows.Input.KeyEventArgs> _keyDown;
        public RelayCommand<System.Windows.Input.KeyEventArgs> KeyDown => _keyDown ?? (_keyDown = new RelayCommand<System.Windows.Input.KeyEventArgs>(keyEventArgs =>
        {
            if (keyEventArgs.Key == Key.Escape) WindowState = WindowState.Minimized;
        }));

        #endregion

        public MainViewModel()
        {
            SetMainAction(StartSnipping, StartSnippingIconPathData);
            MessengerSubscriber();
        }

        #region HelperMethods
        
        private void MessengerSubscriber()
        {
            Messenger.Default.Register<SnippingState>(this, snippingState =>
                                                         {
                                                             switch (snippingState)
                                                             {
                                                                 case SnippingState.Begin:
                                                                     ViewsAccessibility.GetCorresponingWindow(this).Show();
                                                                     SetMainAction(StartSnipping, StartSnippingIconPathData);
                                                                     break;
                                                                 case SnippingState.SelectionStarted:
                                                                     ViewsAccessibility.GetCorresponingWindow(this).Hide();
                                                                     break;
                                                                 case SnippingState.SelectionFinished:
                                                                     ViewsAccessibility.GetCorresponingWindow(this).Show();
                                                                     SetMainAction(SaveSnipping, SaveSnippingIconPathData);
                                                                     break;
                                                             }
                                                         });
        }

        private void SetMainAction(RelayCommand action, string data)
        {
            MainAction = action;
            MainActionIcon = data;
        }

        private Bitmap CaptureSnipping()
        {
            var screen = Screen.PrimaryScreen;
            var bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(screen.Bounds.Left, screen.Bounds.Top),
                    new Point(0, 0), screen.Bounds.Size);
            }
            return bitmap;
        }

        #endregion

        #region Consts

        private const string StartSnippingIconPathData = "M37.130002,45.231999L47.366002,45.231999 47.366002,47.789999 37.130002,47.789999z M16.656001,45.231999L26.893,45.231999 26.893,47.789999 16.656001,47.789999z M61.441411,37.940998L64.000001,37.940998 64.000001,47.789997 52.483998,47.789997 52.483998,45.231471 61.441411,45.231471z M2.581253,36.362995L2.5871734,45.320074 9.877011,45.314875 9.8790004,47.874795 0.029904366,47.879994 0.022000313,36.364296z M61.418999,20.240999L63.979001,20.240999 63.979001,30.476999 61.418999,30.476999z M0,18.572998L2.5589999,18.572998 2.5589999,28.809999 0,28.809999z M54.151101,0L64.000001,0.044281006 63.949898,11.559999 61.390702,11.548298 61.4297,2.591217 54.14,2.5600281z M37.963002,0L48.198998,0 48.198998,2.5599976 37.963002,2.5599976z M17.49,0L27.727,0 27.727,2.5599976 17.49,2.5599976z M0.044281006,0L11.56,0.050811768 11.548899,2.6095428 2.5919122,2.5704689 2.5592221,9.8610001 0,9.8492489z";
        private const string SaveSnippingIconPathData = "M8.1099597,36.94997L8.1099597,41.793968 39.213959,41.793968 39.213959,36.94997z M12.42,0.049999889L18.4,0.049999889 18.4,12.252 12.42,12.252z M0,0L7.9001866,0 7.9001866,14.64218 39.210766,14.64218 39.210766,0 47.401001,0 47.401001,47.917 0,47.917z";

        #endregion
    }
}