using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using InstantSnip.Helpers;
using InstantSnip.Properties;
using Microsoft.Practices.ServiceLocation;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace InstantSnip.ViewModel
{
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

        public bool IsSelecting { get; set; }

        public System.Windows.Point SelectionStartingPosition{ get; set; }

        #endregion


        #region RelayCommands

        public RelayCommand WindowLoaded { get; set; }
        public RelayCommand<MouseButtonEventArgs> MouseLeftButtonDown { get; set; }
        public RelayCommand MouseLeftButtonUp { get; set; }
        public RelayCommand<MouseEventArgs> MouseMove { get; set; }
        
        #endregion

        public ScreenShotViewModel()
        {
            RegisterMessages();
            IniializetRelayCommands();
        }

        #region HelperMethods

        private void RegisterMessages()
        {
            Messenger.Default.Register<Bitmap>(this, ReceiveScreenShotBitmapMessage);
            Messenger.Default.Register<SnippingState>(this, ReceiveSnippingStateMessage);
        }

        private void ReceiveScreenShotBitmapMessage(Bitmap source)
        {
            ScreenShotImageSource = GetBitmapSource(source);
        }

        private void ReceiveSnippingStateMessage(SnippingState state)
        {
             switch (state)
             {
                 case SnippingState.Saved:
                     ViewsAccessibility.GetCorresponingWindow(ServiceLocator.Current.GetInstance<MainViewModel>()).Hide();
                     SaveSnipping();
                     ViewsAccessibility.GetCorresponingWindow(this).Close();
                     break;
             }
        }

        private void SaveSnipping()
        {
            var snip = CaptureSnipping();
            var snipLocation = Settings.Default.SnipLocation;
            var snipName = Settings.Default.SnipName;
            var fileName = snipLocation + "\\" + snipName + ".png";
            if (!Settings.Default.AllowSnipOverwriting)
            {
                var counter = 0;
                do
                {
                    var date = DateTime.Now.ToString("yyyy-MM-dd");
                    snipName = Settings.Default.SnipName + "_" + date + "_" + ++counter;
                    fileName = snipLocation + "\\" + snipName + ".png";
                } while (Directory.GetFiles(snipLocation).Count(name => name == fileName) != 0);
            }
            snip.Save(fileName, ImageFormat.Png);
            Thread.Sleep(600);
            if (Settings.Default.IsCopyImageToClipBoard)
            {
                Clipboard.SetImage(GetBitmapSource(snip));   
            }
            else
            {
                if (Settings.Default.IsCopyUriToClipboard)
                {
                    Clipboard.SetText(fileName);   
                }
            }
            ViewsAccessibility.GetCorresponingWindow(ServiceLocator.Current.GetInstance<MainViewModel>()).WindowState= WindowState.Minimized;
        }

        private void IniializetRelayCommands()
        {
            WindowLoaded = new RelayCommand(() =>
            {
                BackRect = new Rect(0, 0, WindowWidth + 4, WindowHeight + 4);
                SelectionRect = new Rect(0, 0, 0,0);
            });

            MouseLeftButtonDown = new RelayCommand<MouseButtonEventArgs>((e) =>
                                                   {
                                                       Messenger.Default.Send<SnippingState>(SnippingState.SelectionStarted);
                                                       IsSelecting = true;
                                                       var parent = new DependencyObject();
                                                       if (e.Source is System.Windows.Shapes.Path)
                                                       {
                                                           parent = VisualTreeHelper.GetParent((System.Windows.Shapes.Path)e.Source);
                                                       }
                                                       else
                                                       {
                                                           parent = (Canvas)e.Source;
                                                       }
                                                       SelectionStartingPosition = new System.Windows.Point(e.GetPosition((Canvas)parent).X, e.GetPosition((Canvas)parent).Y);
                                                   });

            MouseLeftButtonUp = new RelayCommand(() =>
                                                 {
                                                     IsSelecting = false;
                                                     Messenger.Default.Send<SnippingState>(SnippingState.SelectionFinished);
                                                 });

            MouseMove = new RelayCommand<MouseEventArgs>(PerformSnipping);
        }

        private void PerformSnipping(MouseEventArgs e)
        {
            if (!IsSelecting) return;
            var parent = new DependencyObject();
            if (e.Source is System.Windows.Shapes.Path)
            {
                parent = VisualTreeHelper.GetParent((System.Windows.Shapes.Path) e.Source);
            }
            else
            {
                parent = (Canvas) e.Source;
            }

            // From here and on the code is bad, I'll inhance it

            double selectionWidth = 0;
            double selectionHeight = 0;

            double RectLeft = 0;
            double RectTop = 0;

            if ((e.GetPosition(parent as Canvas).X > SelectionStartingPosition.X) &&
                (e.GetPosition(parent as Canvas).Y > SelectionStartingPosition.Y))
            {
                selectionWidth = e.GetPosition(parent as Canvas).X -
                                 SelectionStartingPosition.X;
                selectionHeight = e.GetPosition(parent as Canvas).Y -
                                  SelectionStartingPosition.Y;
                RectLeft = SelectionStartingPosition.X;
                RectTop = SelectionStartingPosition.Y;
            }
            else
            {
                if ((e.GetPosition(parent as Canvas).X < SelectionStartingPosition.X) &&
                    (e.GetPosition(parent as Canvas).Y < SelectionStartingPosition.Y))
                {
                    RectLeft = e.GetPosition(parent as Canvas).X;
                    RectTop = e.GetPosition(parent as Canvas).Y;

                    selectionWidth = SelectionStartingPosition.X - RectLeft;
                    selectionHeight = SelectionStartingPosition.Y - RectTop;
                }
                else
                {
                    if (e.GetPosition(parent as Canvas).X < SelectionStartingPosition.X)
                    {
                        RectLeft = e.GetPosition(parent as Canvas).X;
                        RectTop = SelectionStartingPosition.Y;

                        selectionWidth = SelectionStartingPosition.X - RectLeft;
                        selectionHeight = e.GetPosition(parent as Canvas).Y -
                                          SelectionStartingPosition.Y;
                    }
                    else
                    {
                        if (e.GetPosition(parent as Canvas).Y < SelectionStartingPosition.Y)
                        {
                            RectLeft = SelectionStartingPosition.X;
                            RectTop = e.GetPosition(parent as Canvas).Y;

                            selectionWidth = e.GetPosition(parent as Canvas).X -
                                             SelectionStartingPosition.X;
                            selectionHeight = SelectionStartingPosition.Y - RectTop;
                        }
                    }
                }
            }

            SelectionRect = new Rect(RectLeft, RectTop, selectionWidth, selectionHeight);
        }

        private Bitmap CaptureSnipping()
        {
            var bitmap = new Bitmap((int)SelectionRect.Width -2, (int)SelectionRect.Height -2);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point((int) SelectionRect.X, (int) SelectionRect.Y),
                    new Point(0, 0), bitmap.Size);
            }
            return bitmap;
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr obj);

        public static BitmapSource GetBitmapSource(Bitmap source)
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