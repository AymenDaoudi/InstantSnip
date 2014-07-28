using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;

namespace InstantSnip.Helpers
{
    public static class ViewsAccessibility
    {
        public static Window GetCorresponingWindow(ViewModelBase viewModel)
        {
            var windowAccessibility = new WindowAccessibility(viewModel);
            return windowAccessibility.CorrespondanteWindow;

        }

        private class WindowAccessibility 
        {
            #region Properties

            public Window CorrespondanteWindow { get; private set; }

            #endregion


            public WindowAccessibility(ViewModelBase viewModel)
            {
                var windows = Application.Current.Windows.OfType<Window>();

                CorrespondanteWindow = (from window in windows
                                          where window.DataContext.Equals(viewModel)
                                          select window).First();
            }

            #region Fields

            #endregion

            
        }
    }
}
