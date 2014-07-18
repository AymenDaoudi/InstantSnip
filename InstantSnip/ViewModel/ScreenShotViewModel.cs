using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
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

        private ImageSource _screenShotImageSource;

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

        public ScreenShotViewModel()
        {
            Messenger.Default.Register<Bitmap>(this, HandleScreenShotBitmap);
        }

        private void HandleScreenShotBitmap(Bitmap source)
        {
            ScreenShotImageSource = LoadBitmap(source);
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
    }
}