using System;
using System.Linq;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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

        private void InitializeRelayCommands()
        {
            WindowLoaded = new RelayCommand(() =>
                                            {
                                                SnipName = Settings.Default.SnipName;
                                                SnipLocation = Settings.Default.SnipLocation;
                                                AllowSnipOverwriting = Settings.Default.AllowSnipOverwriting;
                                                AllowDeletingPictureAfterSnipping = Settings.Default.AllowDeletingPictureAfterSnipping;
                                                TimeBeforeDeletingPicture = Settings.Default.TimeBeforeDeletingPicture;
                                            });

            ChangeLocation = new RelayCommand(() =>
                                              {
                                                  var folderBrowserDialog = new FolderBrowserDialog { ShowNewFolderButton = true };
                                                  folderBrowserDialog.ShowDialog();
                                                  SnipLocation = folderBrowserDialog.SelectedPath;
                                              });

            SaveSettings = new RelayCommand(() =>
                                            {
                                                Settings.Default.TimeBeforeDeletingPicture = TimeBeforeDeletingPicture;
                                                Settings.Default.SnipName = SnipName;
                                                Settings.Default.SnipLocation = SnipLocation;
                                                Settings.Default.AllowDeletingPictureAfterSnipping =
                                                    AllowDeletingPictureAfterSnipping;
                                                Settings.Default.AllowSnipOverwriting = AllowSnipOverwriting;
                                                foreach (var window in Application.Current.Windows.OfType<SettingsView>())
                                                {
                                                    window.Close();
                                                    break;
                                                }
                                            });

            CancelSettings = new RelayCommand(() =>
                                            {
                                                TimeBeforeDeletingPicture = Settings.Default.TimeBeforeDeletingPicture;
                                                SnipName = Settings.Default.SnipName;
                                                SnipLocation = Settings.Default.SnipLocation;
                                                AllowDeletingPictureAfterSnipping = Settings.Default.AllowDeletingPictureAfterSnipping ;
                                                AllowSnipOverwriting = Settings.Default.AllowSnipOverwriting ;
                                                foreach (var window in Application.Current.Windows.OfType<SettingsView>())
                                                {
                                                    window.Close();
                                                    break;
                                                }
                                            });
        }

        #region Fields

        private string _snipName;
        private string _snipLocation;
        private bool _allowSnipOverwriting;
        private bool _allowDeletingPictureAfterSnipping;
        private TimeSpan _timeBeforeDeletingPicture;

        #endregion
    }
}