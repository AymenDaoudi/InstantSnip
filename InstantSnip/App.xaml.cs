using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Threading;
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
            Settings.Default.SnipLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}
