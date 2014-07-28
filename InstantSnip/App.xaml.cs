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
            Settings.Default.SnipLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}
