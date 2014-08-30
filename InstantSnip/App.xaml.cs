using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using InstantSnip.Helpers;
using InstantSnip.Properties;

namespace InstantSnip
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Properties
        public static Timer TimeBeforeDeletingSpan
        {
            get { return _timeBeforeDeletingSpan; }
            set { _timeBeforeDeletingSpan = value; }
        }

        public static List<string> ListOfCapturesUriPaths
        {
            get { return _listOfCapturesUriPaths; }
            set { _listOfCapturesUriPaths = value; }
        }

        public static bool IsTimerTimerDead { get; set; }

        #endregion

        static App()
        {
            DispatcherHelper.Initialize();
            ListOfCapturesUriPaths = new List<string>();
            if (Settings.Default.AllowDeletingPictureAfterSnipping) SetSnipDeletingTimer(Settings.Default.TimeBeforeDeletingPicture);
        }

        public static void SetSnipDeletingTimer(double d)
        {
            TimeBeforeDeletingSpan = new Timer(d) {AutoReset = true, Enabled = true};
            TimeBeforeDeletingSpan.Elapsed += TimeBeforeDeletingSpan_Elapsed;
            TimeBeforeDeletingSpan.Start();
            IsTimerTimerDead = false;
            TimeBeforeDeletingSpan.Disposed += (sender, args) => IsTimerTimerDead = true;
        }

        static void TimeBeforeDeletingSpan_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var t in ListOfCapturesUriPaths) File.Delete(t);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            WpfSingleInstance.Make();

            
            if (Settings.Default.IsFirstRun)
            {
                Settings.Default.IsFirstRun = false;
                Settings.Default.SnipName = "Capture";
                Settings.Default.SnipLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\InstantSnip Captures";
                Settings.Default.AllowSnipOverwriting = true;
                Settings.Default.AllowDeletingPictureAfterSnipping = false;
                Settings.Default.IsCopyImageToClipBoard = true;
                Settings.Default.IsCopyUriToClipboard= false;
                Settings.Default.TimeBeforeDeletingPicture = 0;
                Settings.Default.Save();
            }


            

        }


        #region Fields

        private static Timer _timeBeforeDeletingSpan;
        private static List<String> _listOfCapturesUriPaths;

        #endregion

    }
}
