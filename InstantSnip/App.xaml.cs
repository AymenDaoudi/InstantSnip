using System;
using System.Windows;
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
        static App()
        {
            DispatcherHelper.Initialize();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            WpfSingleInstance.Make();
            this.Properties.Add("SnipName", "Capture");
            this.Properties.Add("SnipLocation",Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            this.Properties.Add("AllowSnipOverwriting", true);
            this.Properties.Add("AllowDeletingPictureAfterSnipping", false);
            this.Properties.Add("TimeBeforeDeletingPicture", new TimeSpan(0,0,0));
            this.Properties.Add("IsCopyImageToClipBoard", true);
            this.Properties.Add("IsCopyUriToClipboard", false);
        }
    }
}
