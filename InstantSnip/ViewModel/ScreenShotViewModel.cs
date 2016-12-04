using System;
using System.Drawing;
using System.Drawing.Imaging;
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
using Microsoft.Practices.ServiceLocation;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace InstantSnip.ViewModel
{
    public class ScreenShotViewModel : ViewModelBase
    {

        #region Properties

        private ImageSource _screenShotImageSource;
        public ImageSource ScreenShotImageSource
        {
            get
            {
                return _screenShotImageSource;
            }
            set
            {
                if (_screenShotImageSource == value) return;
                _screenShotImageSource = value;
                RaisePropertyChanged(nameof(ScreenShotImageSource));
            }
        }

        private Rect _backgroundRect;
        public Rect BackgroundRect
        {
            get { return _backgroundRect; }
            set
            {
                if (_backgroundRect == value) return;
                _backgroundRect = value;
                RaisePropertyChanged(nameof(BackgroundRect));
            }
        }

        private Rect _selectionRect;
        public Rect SelectionRect
        {
            get { return _selectionRect; }
            set
            {
                if (_selectionRect == value) return;
                _selectionRect = value;
                RaisePropertyChanged(nameof(SelectionRect));
            }
        }

        private double _windowWidth;
        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                if (_windowWidth == value) return;
                _windowWidth = value;
                RaisePropertyChanged(nameof(WindowWidth));
            }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                if (_windowHeight == value) return;
                _windowHeight = value;
                RaisePropertyChanged(nameof(WindowHeight));
            }
        }

        private Cursor _snippingCursor;
        public Cursor SnippingCursor
        {
            get { return _snippingCursor; }
            set
            {
                _snippingCursor = value;
                RaisePropertyChanged(nameof(SnippingCursor));
            }
        }

        public bool IsSelecting { get; set; }

        public System.Windows.Point SelectionStartingPosition{ get; set; }

        #endregion

        #region RelayCommands

        private RelayCommand _windowLoaded;

        public RelayCommand WindowLoaded => _windowLoaded ?? (_windowLoaded = new RelayCommand(() =>
                                                                                                   {
                                                                                                       BackgroundRect = new Rect(0, 0, WindowWidth + 4, WindowHeight + 4);
                                                                                                       SelectionRect = new Rect(0, 0, 0, 0);
                                                                                                   }));

        private RelayCommand<MouseButtonEventArgs> _mouseLeftButtonDown;
        public RelayCommand<MouseButtonEventArgs> MouseLeftButtonDown => _mouseLeftButtonDown ?? (_mouseLeftButtonDown = new RelayCommand<MouseButtonEventArgs>((e) =>
                                                                                                   {
                                                                                                       Messenger.Default.Send(SnippingState.SelectionStarted);
                                                                                                       IsSelecting = true;
                                                                                                       var parent = GetPathParent(e);
                                                                                                       SelectionStartingPosition = new System.Windows.Point(e.GetPosition(parent).X, e.GetPosition(parent).Y);
                                                                                                   }));

        private RelayCommand<MouseButtonEventArgs> _mouseLeftButtonUp;

        public RelayCommand<MouseButtonEventArgs> MouseLeftButtonUp => _mouseLeftButtonUp ?? (_mouseLeftButtonUp = new RelayCommand<MouseButtonEventArgs>((e) =>
                                                                                                   {
                                                                                                       SnippingCursor = Cursors.Arrow;
                                                                                                       IsSelecting = false;
                                                                                                       Messenger.Default.Send(SnippingState.SelectionFinished);
                                                                                                   }));

        private RelayCommand<MouseEventArgs> _mouseMove;
        public RelayCommand<MouseEventArgs> MouseMove => _mouseMove ?? (_mouseMove = new RelayCommand<MouseEventArgs>(PerformSnipping));

        #endregion

        public ScreenShotViewModel()
        {
            RegisterMessages();
        }

        #region HelperMethods

        private void SetCrossCursor()
        {
            var crossCursorStream = Application.GetResourceStream(new Uri("pack://application:,,,/Images/Cursor_Cross.cur"))?.Stream;
            crossCursorStream = GetCursorFromCUR(crossCursorStream, 17, 17);
            SnippingCursor = new Cursor(crossCursorStream);
        }

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
                     ViewsAccessibility.GetCorresponingWindow(ServiceLocator.Current.GetInstance<MainViewModel>()).WindowState = WindowState.Minimized;
                     SaveSnipping();
                     ViewsAccessibility.GetCorresponingWindow(this).Close();
                     break;
             }
        }

        private void SaveSnipping()
        {
            var snip = CaptureSnipping();
            var snipLocation = Settings.Default.SnipLocation;
            if (!Directory.Exists(Settings.Default.SnipLocation)) Directory.CreateDirectory(Settings.Default.SnipLocation);            
            var snipName = Settings.Default.SnipName;
            var fileName = string.Concat(snipLocation, "\\", snipName, ".png");
            if (!Settings.Default.AllowSnipOverwriting)
            {
                var counter = 0;
                do
                {
                    var date = DateTime.Now.ToString("yyyy-MM-dd");
                    snipName = Settings.Default.SnipName +"_" + date + "_" + ++counter;
                    fileName = snipLocation + "\\" + snipName + ".png";
                } while (Directory.GetFiles(snipLocation).Contains(fileName));
            }
            snip.Save(fileName, ImageFormat.Png);
            Thread.Sleep(200);

            if (Settings.Default.IsCopyImageToClipBoard) Clipboard.SetImage(GetBitmapSource(snip));   
            else if (Settings.Default.IsCopyUriToClipboard) Clipboard.SetText(fileName);
            
            RecycleSnipLocation(fileName);
        }

        private static void RecycleSnipLocation(string fileName)
        {
            App.ListOfCapturesUriPaths.Add(fileName);
            if ((App.IsTimerTimerDead)||(App.TimeBeforeDeletingSpan==null)) return;            
            App.TimeBeforeDeletingSpan.Stop();
            App.TimeBeforeDeletingSpan.Start();
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
            SetCrossCursor();
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

    }
}