using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace InstantSnip.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ScreenShotViewModel : ViewModelBase
    {
        #region Properties
        public ImageSource ScreenShotImageSource
        {
            get
            {
                return _screenShotImageSource;
            }
            set
            {
                _screenShotImageSource = value;
                RaisePropertyChanged("ScreenShotImageSource");
            }
        }


        public Rect BackRect
        {
            get { return _backRect; }
            set
            {
                _backRect = value;
                RaisePropertyChanged("BackRect");
            }
        }

        public Rect SelectionRect
        {
            get { return _selectionRect; }
            set
            {
                _selectionRect = value;
                RaisePropertyChanged("SelectionRect");
            }
        }

        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                _windowWidth = value;
                RaisePropertyChanged("WindowWidth");
            }
        }

        public double WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                _windowHeight = value;
                RaisePropertyChanged("WindowHeight");
            }
        }

        #endregion


        #region RelayCommands

        public RelayCommand WindowLoaded { get; set; }
        #endregion


        public ScreenShotViewModel()
        {
            MessengerSubscriber();
            InitRelayCommands();
        }

    

        #region HelperMethods

        private void MessengerSubscriber()
        {
            Messenger.Default.Register<Bitmap>(this, HandleScreenShotBitmap);
        }


        private void HandleScreenShotBitmap(Bitmap source)
        {
            ScreenShotImageSource = LoadBitmap(source);
        }


        private void InitRelayCommands()
        {
            WindowLoaded = new RelayCommand(() =>
            {
                BackRect = new Rect(0, 0, WindowWidth, WindowHeight);
                SelectionRect = new Rect(10, 10, 222, 222);
            });
        }


        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr obj);

        public static BitmapSource LoadBitmap(Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bitmapSource = null;
            try
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bitmapSource;
        }
    
        #endregion
        


        #region Fields
            private ImageSource _screenShotImageSource;
            private Rect _backRect;
            private Rect _selectionRect;
            private double _windowWidth;
            private double _windowHeight;

        #endregion
    }
}