using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
                BackRect = new Rect(0, 0, WindowWidth + 4, WindowHeight+ 4);
                SelectionRect = new Rect(0, 0, 0,0);
            });

            MouseLeftButtonDown = new RelayCommand<MouseButtonEventArgs>((e) =>
                                                   {
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
                                                     CaptureSelection();
                                                 });

            MouseMove = new RelayCommand<MouseEventArgs>((e) =>
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
                             });
        }


        private Bitmap CaptureSelection()
        {
            var screen = Screen.PrimaryScreen;
            var bitmap = new Bitmap((int)SelectionRect.Width -2, (int)SelectionRect.Height -2);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point((int) SelectionRect.X, (int) SelectionRect.Y),
                    new Point(0, 0), bitmap.Size);
            }

            bitmap.Save(@"C:\Users\Aymen\Desktop\test.png",ImageFormat.Png);
            return bitmap;
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