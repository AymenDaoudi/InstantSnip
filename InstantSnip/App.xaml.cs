using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Threading;

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
    }
}
