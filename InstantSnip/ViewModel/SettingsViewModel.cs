using System;
using System.Linq;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using InstantSnip.Helpers;
using InstantSnip.Properties;
using InstantSnip.Views;
using Application = System.Windows.Application;

namespace InstantSnip.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {

        #region Properties

        public string SnipName
        {
            get { return _snipName; }
            set
            {
                _snipName = value;
                RaisePropertyChanged("SnipName");
            }
        }

        public string SnipLocation
        {
            get { return _snipLocation; }
            set
            {
                _snipLocation = value;
                RaisePropertyChanged("SnipLocation");
            }
        }

        public bool AllowSnipOverwriting
        {
            get { return _allowSnipOverwriting; }
            set
            {
                _allowSnipOverwriting = value;
                RaisePropertyChanged("AllowSnipOverwriting");
            }
        }

        public bool AllowDeletingPictureAfterSnipping
        {
            get { return _allowDeletingPictureAfterSnipping; }
            set
            {
                _allowDeletingPictureAfterSnipping = value;
                RaisePropertyChanged("AllowDeletingPictureAfterSnipping");
            }
        }

        public TimeSpan TimeBeforeDeletingPicture
        {
            get { return _timeBeforeDeletingPicture; }
            set
            {
                _timeBeforeDeletingPicture = value;
                RaisePropertyChanged("TimeBeforeDeletingPicture");
            }
        }

        public bool IsCopyImageToClipBoard
        {
            get { return _isCopyImageToClipBoard; }
            set
            {
                _isCopyImageToClipBoard = value;
                IsCopyURIToClipboard = !value;
                RaisePropertyChanged("IsCopyImageToClipBoard");
            }
        }

        public bool IsCopyURIToClipboard
        {
            get { return _isCopyURIToClipboard; }
            set
            {
                _isCopyURIToClipboard = value;
                _isCopyImageToClipBoard = !value;
                RaisePropertyChanged("IsCopyURIToClipboard");
            }
        }

        #endregion

        #region RelayCommands

        public RelayCommand WindowLoaded { get; set; }
        public RelayCommand ChangeLocation { get; set; }
        public RelayCommand SaveSettings { get; set; }
        public RelayCommand CancelSettings { get; set; }

        #endregion

        public SettingsViewModel()
        {
            InitializeRelayCommands();
        }

        #region HelperMethods
            private void InitializeRelayCommands()
        {
            WindowLoaded = new RelayCommand(LoadProperties);

            ChangeLocation = new RelayCommand(() =>
                                              {
                                                  var folderBrowserDialog = new FolderBrowserDialog { ShowNewFolderButton = true };
                                                  folderBrowserDialog.ShowDialog();
                                                  SnipLocation = folderBrowserDialog.SelectedPath;
                                              });

            SaveSettings = new RelayCommand(() =>
                                            {
                                                Application.Current.Properties.Add("SnipName", SnipName);
                                                Application.Current.Properties.Add("SnipLocation", SnipLocation);
                                                Application.Current.Properties.Add("AllowSnipOverwriting", AllowSnipOverwriting);
                                                Application.Current.Properties.Add("AllowDeletingPictureAfterSnipping", AllowDeletingPictureAfterSnipping);
                                                Application.Current.Properties.Add("TimeBeforeDeletingPicture", TimeBeforeDeletingPicture);
                                                Application.Current.Properties.Add("IsCopyImageToClipBoard", IsCopyImageToClipBoard);
                                                Application.Current.Properties.Add("IsCopyUriToClipboard", IsCopyURIToClipboard);

                                                ViewsAccessibility.GetCorresponingWindow(this).Close();
                                            });

            CancelSettings = new RelayCommand(() =>
                                              {
                                                  LoadProperties();
                                                  ViewsAccessibility.GetCorresponingWindow(this).Close();
                                            });
        }

            private void LoadProperties()
        {
            SnipName = (String) Application.Current.Properties["SnipName"];
            SnipLocation = (String) Application.Current.Properties["SnipLocation"];
            AllowSnipOverwriting = (bool) Application.Current.Properties["AllowSnipOverwriting"];
            AllowDeletingPictureAfterSnipping = (bool) Application.Current.Properties["AllowDeletingPictureAfterSnipping"];
            TimeBeforeDeletingPicture = (TimeSpan) Application.Current.Properties["TimeBeforeDeletingPicture"];
            IsCopyImageToClipBoard = (bool) Application.Current.Properties["IsCopyImageToClipBoard"];
            IsCopyURIToClipboard = (bool) Application.Current.Properties["IsCopyUriToClipboard"];
        }

        #endregion

        #region Fields

        private string _snipName;
        private string _snipLocation;
        private bool _allowSnipOverwriting;
        private bool _allowDeletingPictureAfterSnipping;
        private TimeSpan _timeBeforeDeletingPicture;
        private bool _isCopyImageToClipBoard;
        private bool _isCopyURIToClipboard;

        #endregion
    }
}