using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using InstantSnip.Views;
using Application = System.Windows.Application;

namespace InstantSnip.Helpers
{
    public static class SystemTrayMinimization
    {

        #region Fields
        private static Window _window;
        private static NotifyIcon _notifyIcon;
        private static bool _balloonShown;
        #endregion

        #region Methods
        public static void Enable(Window window)
        {
            _window = window;
            if (_notifyIcon == null) _notifyIcon = SetNotificationIcon();
            _balloonShown = true;
            _window.StateChanged += new EventHandler(HandleStateChanged);
        }

        private static NotifyIcon SetNotificationIcon()
        {
            var notifyIcon = new NotifyIcon()
            {
                Icon = GetApplicationIcon(),
                Text = _window.Title,
                Visible = true
            };
            notifyIcon.ShowBalloonTip(600, null, _window.Title, ToolTipIcon.None);
            notifyIcon.DoubleClick += new EventHandler(HandleNotifyIconOrBalloonClicked);
            notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
            notifyIcon.ContextMenu = SetContextMenu();
            return notifyIcon;
        }

        private static ContextMenu SetContextMenu()
        {
            var openMenuItem = new MenuItem() { Index = 0, Text = "Open" };
            openMenuItem.Click += new EventHandler(HandleNotifyIconOrBalloonClicked);

            var settingsMenuItem = new MenuItem() { Index = 1, Text = "Settings" };
            settingsMenuItem.Click += (o, args) =>
            {
                var settingsView = new SettingsView();
                settingsView.Show();
            };

            var exitMenuItem = new MenuItem() { Index = 1, Text = "Exit" };
            exitMenuItem.Click += (o, args) => Application.Current.Shutdown();

            return new ContextMenu(new MenuItem[] { openMenuItem, settingsMenuItem, exitMenuItem });
        }

        private static Icon GetApplicationIcon()
        {
            Icon applicationIcon;

            //var applicationPath = Assembly.GetExecutingAssembly().Location;
            //var directoryPath = Path.GetDirectoryName(applicationPath);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)_window.Icon));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;
                var bitmap = (Bitmap)Image.FromStream(stream);
                applicationIcon = Icon.FromHandle(bitmap.GetHicon());
            }
            return applicationIcon;
        }
        #endregion

        #region Events
        private static void HandleStateChanged(object sender, EventArgs e)
        {

            var minimized = (_window.WindowState == WindowState.Minimized);
            _window.ShowInTaskbar = !minimized;
        }

        private static void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
        {
            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    _window.WindowState = WindowState.Minimized;
                    break;
                case WindowState.Minimized:
                    _window.WindowState = WindowState.Normal;
                    break;
            }
        }

        #endregion
        
    }
}
