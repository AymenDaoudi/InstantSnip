using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using InstantSnip.Views;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;

namespace InstantSnip.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Properties

        public Visibility WindowVisibility
        {
            get
            {
                return _windowVisibility;
            }
            set
            {
                _windowVisibility = value;
                RaisePropertyChanged("WindowVisibility");
            }
        }
        #endregion

        #region RelayCommands

        public RelayCommand CloseApplication
        {
            get;
            private set;
        }

        public RelayCommand StartSnipping
        {
            get;
            private set;
        }

        #endregion
        
        public MainViewModel()
        {
            InitRelayCommands();
            MessengerSubscriber();
        }

        #region HelperMethods
        private void InitRelayCommands()
        {
            CloseApplication = new RelayCommand(() => Application.Current.Shutdown());
            StartSnipping = new RelayCommand(() =>
                                             {
                                                 Application.Current.MainWindow.Hide();

                                                 var bitmap = CaptureScreen();

                                                 //bitmap.Save(@"C:/Users/Aymen/Desktop/test.png", ImageFormat.Png);
                                                                            
                                                 var screenShotWindow = new ScreeShotView();
                                                 screenShotWindow.Show();
                                                 Messenger.Default.Send(bitmap);

                                                 Application.Current.MainWindow.Show();
                                             });
        }

        private Bitmap CaptureScreen()
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

        private void MessengerSubscriber()
        {
            Messenger.Default.Register<Visibility>(this, visibility => WindowVisibility = visibility);
        }

        #endregion


        #region Fields
        private Visibility _windowVisibility;
        #endregion
    }
}