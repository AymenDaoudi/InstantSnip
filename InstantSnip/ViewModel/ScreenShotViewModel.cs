using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
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
        public Rect BackgroundRect
        {
            get { return _backgroundRect; }
            set
            {
                _backgroundRect = value;
                RaisePropertyChanged("BackgroundRect");
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
        public Cursor SnippingCursor
        {
            get { return _snippingCursor; }
            set
            {
                _snippingCursor = value;
                RaisePropertyChanged("SnippingCursor");
            }
        }
        #endregion


        #region RelayCommands

        public RelayCommand WindowLoaded { get; set; }
        public RelayCommand<MouseButtonEventArgs> MouseLeftButtonDown { get; set; }
        public RelayCommand<MouseButtonEventArgs> MouseLeftButtonUp { get; set; }
        public RelayCommand<MouseEventArgs> MouseMove { get; set; }

     
        #endregion

        public ScreenShotViewModel()
        {
            var crossCursorStream = Application.GetResourceStream(new Uri("pack://application:,,,/Images/Cursor_Cross.cur")).Stream;
            crossCursorStream = GetCursorFromCUR(crossCursorStream, 17, 17);
            SnippingCursor = new Cursor(crossCursorStream);
            RegisterMessages();
            InializetRelayCommands();
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
            var snipLocation = (String)Application.Current.Properties["SnipLocation"];
            var snipName = (String)Application.Current.Properties["SnipName"];
            var fileName = snipLocation + "\\" + snipName + ".png";
            if (!(bool) Application.Current.Properties["AllowSnipOverwriting"])
            {
                var counter = 0;
                do
                {
                    var date = DateTime.Now.ToString("yyyy-MM-dd");
                    snipName = (String)Application.Current.Properties["SnipName"] +"_" + date + "_" + ++counter;
                    fileName = snipLocation + "\\" + snipName + ".png";
                } while (Directory.GetFiles(snipLocation).Count(name => name == fileName) != 0);
            }
            snip.Save(fileName, ImageFormat.Png);
            Thread.Sleep(600);
            if ((bool) Application.Current.Properties["IsCopyImageToClipBoard"])
            {
                Clipboard.SetImage(GetBitmapSource(snip));   
            }
            else
            {
                if ((bool)Application.Current.Properties["IsCopyUriToClipboard"])
                {
                    Clipboard.SetText(fileName);   
                }
            }
            ViewsAccessibility.GetCorresponingWindow(ServiceLocator.Current.GetInstance<MainViewModel>()).WindowState= WindowState.Minimized;
        }

        private void InializetRelayCommands()
        {
            WindowLoaded = new RelayCommand(() =>
            {
                BackgroundRect = new Rect(0, 0, WindowWidth + 4, WindowHeight + 4);
                SelectionRect = new Rect(0, 0, 0,0);
            });

            MouseLeftButtonDown = new RelayCommand<MouseButtonEventArgs>((e) =>
                                                   {
                                                       Messenger.Default.Send(SnippingState.SelectionStarted);
                                                       IsSelecting = true;
                                                       var parent = GetPathParent(e);
                                                       SelectionStartingPosition = new System.Windows.Point(e.GetPosition(parent).X, e.GetPosition(parent).Y);
                                                   });

            MouseLeftButtonUp = new RelayCommand<MouseButtonEventArgs>((e) =>
                                                 {
                                                     SnippingCursor = Cursors.Arrow;
                                                     IsSelecting = false;
                                                     Messenger.Default.Send(SnippingState.SelectionFinished);
                                                 });

            MouseMove = new RelayCommand<MouseEventArgs>(PerformSnipping);
        }

        private static Canvas GetPathParent(MouseButtonEventArgs e)
        {
            Canvas parent;
            if (e.Source is System.Windows.Shapes.Path)
            {
                parent = (Canvas)VisualTreeHelper.GetParent((System.Windows.Shapes.Path)e.Source);
            }
            else
            {
                parent = (Canvas)e.Source;
            }
            return parent;
        }
        private static Canvas GetPathParent(MouseEventArgs e)
        {
            Canvas parent;
            if (e.Source is System.Windows.Shapes.Path)
            {
                parent = (Canvas)VisualTreeHelper.GetParent((System.Windows.Shapes.Path)e.Source);
            }
            else
            {
                parent = (Canvas)e.Source;
            }
            return parent;
        }

        private void PerformSnipping(MouseEventArgs e)
        {
            if (!IsSelecting) return;
            var parent = GetPathParent(e);
            // From here and on the code is bad, I'll inhance it

            double selectionWidth = 0;
            double selectionHeight = 0;

            double RectLeft = 0;
            double RectTop = 0;

            if ((e.GetPosition(parent).X > SelectionStartingPosition.X) &&
                (e.GetPosition(parent).Y > SelectionStartingPosition.Y))
            {
                selectionWidth = (e.GetPosition(parent).X -
                                 SelectionStartingPosition.X);
                selectionHeight = (e.GetPosition(parent).Y -
                                  SelectionStartingPosition.Y);
                RectLeft = SelectionStartingPosition.X;
                RectTop = SelectionStartingPosition.Y;
            }
            else
            {
                if ((e.GetPosition(parent).X < SelectionStartingPosition.X) &&
                    (e.GetPosition(parent).Y < SelectionStartingPosition.Y))
                {
                    RectLeft = e.GetPosition(parent).X;
                    RectTop = e.GetPosition(parent).Y;

                    selectionWidth = SelectionStartingPosition.X - RectLeft;
                    selectionHeight = SelectionStartingPosition.Y - RectTop;
                }
                else
                {
                    if (e.GetPosition(parent).X < SelectionStartingPosition.X)
                    {
                        RectLeft = e.GetPosition(parent).X;
                        RectTop = SelectionStartingPosition.Y;

                        selectionWidth = SelectionStartingPosition.X - RectLeft;
                        selectionHeight = e.GetPosition(parent).Y -
                                          SelectionStartingPosition.Y;
                    }
                    else
                    {
                        if (e.GetPosition(parent).Y < SelectionStartingPosition.Y)
                        {
                            RectLeft = SelectionStartingPosition.X;
                            RectTop = e.GetPosition(parent).Y;

                            selectionWidth = e.GetPosition(parent).X -
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

        public static Stream GetCursorFromCUR(Stream stream, byte hotspotx, byte hotspoty)
        {
            var buffer = new byte[stream.Length];

            stream.Read(buffer, 0, (int)stream.Length);
            var ms = new MemoryStream();

            buffer[10] = hotspotx;
            buffer[12] = hotspoty;

            ms.Write(buffer, 0, (int)stream.Length);
            ms.Position = 0;

            return ms;
        }

        #endregion

        #region Fields
            private ImageSource _screenShotImageSource;
            private Rect _backgroundRect;
            private Rect _selectionRect;
            private double _windowWidth;
            private double _windowHeight;
            private Cursor _snippingCursor;

        #endregion
    }
}