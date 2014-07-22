﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace InstantSnip.Helpers
{
    public static class SystemTrayMinimization
    {
        public static void Enable(Window window)
        {
            new SystemTrayMinimizationInstance(window);
        }

        private class SystemTrayMinimizationInstance
        {
            public SystemTrayMinimizationInstance(Window window)
            {
                _window = window;
                _window.StateChanged += new EventHandler(HandleStateChanged);
            }

            private void HandleStateChanged(object sender, EventArgs e)
            {
                if (_notifyIcon == null) _notifyIcon = SetNotificationIcon();
                
                var minimized = (_window.WindowState == WindowState.Minimized);
                _window.ShowInTaskbar = !minimized;
                _notifyIcon.Visible = minimized;
                
                if (!minimized || _balloonShown) return;

                _notifyIcon.ShowBalloonTip(1000, null, _window.Title, ToolTipIcon.None);
                _balloonShown = true;
            }

            private NotifyIcon SetNotificationIcon()
            {
                var notifyIcon = new NotifyIcon()
                                 {
                                     Icon = GetApplicationIcon(),
                                     Text = _window.Title
                                 };
                notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
                notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
                notifyIcon.ContextMenu = SetContextMenu();
                return notifyIcon;
            }

            private ContextMenu SetContextMenu()
            {
                var openMenuItem = new MenuItem()
                                   {
                                       Index = 0,
                                       Text = "Open"
                                   };
                openMenuItem.Click += new EventHandler(HandleNotifyIconOrBalloonClicked);
                var settingsMenuItem = new MenuItem()
                                       {
                                           Index = 1,
                                           Text = "Settings"
                                       };
                settingsMenuItem.Click += new EventHandler(HandleNotifyIconOrBalloonClicked);
                var exitMenuItem = new MenuItem()
                                   {
                                       Index = 1,
                                       Text = "Settings"
                                   };
                exitMenuItem.Click += (o, args) => Application.Current.Shutdown();

                var contextMenu = new ContextMenu();
                contextMenu.MenuItems.AddRange(new MenuItem[] {openMenuItem, settingsMenuItem, exitMenuItem});
                return contextMenu;
            }

            private Icon GetApplicationIcon()
            {
                Icon applicationIcon;

                var applicationPath = Assembly.GetExecutingAssembly().Location;
                var directoryPath = Path.GetDirectoryName(applicationPath);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource) _window.Icon));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Position = 0; // rewind the stream
                    var bitmap = (Bitmap) Image.FromStream(stream);
                    applicationIcon = Icon.FromHandle(bitmap.GetHicon());
                }
                return applicationIcon;
            }

            private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
            {
                _window.WindowState = WindowState.Normal;
            }

            #region Fields
            private readonly Window _window;
            private NotifyIcon _notifyIcon;
            private bool _balloonShown;
            #endregion


        }
    }
}
