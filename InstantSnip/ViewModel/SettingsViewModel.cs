using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using InstantSnip.Helpers;

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

        public double TimeBeforeDeletingPicture
        {
            get { return _timeBeforeDeletingPicture; }
            set
            {
                _timeBeforeDeletingPicture = value;
                Messenger.Default.Send<double>(value);
                RaisePropertyChanged("TimeBeforeDeletingPicture");
            }
        }

        public bool IsCopyImageToClipBoard
        {
            get { return _isCopyImageToClipBoard; }
            set
            {
                _isCopyImageToClipBoard = value;
                RaisePropertyChanged("IsCopyImageToClipBoard");
            }
        }

        public bool IsCopyURIToClipboard
        {
            get { return _isCopyURIToClipboard; }
            set
            {
                _isCopyURIToClipboard = value;
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
                                                    Settings.Default.SnipName = SnipName;
                                                    Settings.Default.SnipLocation = SnipLocation;
                                                    Settings.Default.AllowSnipOverwriting = AllowSnipOverwriting;
                                                    Settings.Default.AllowDeletingPictureAfterSnipping = AllowDeletingPictureAfterSnipping;
                                                    Settings.Default.TimeBeforeDeletingPicture = TimeBeforeDeletingPicture;
                                                    Settings.Default.IsCopyImageToClipBoard = IsCopyImageToClipBoard;
                                                    Settings.Default.IsCopyUriToClipboard = IsCopyURIToClipboard ;
                                                    Settings.Default.Save();

                                                    if (Settings.Default.AllowDeletingPictureAfterSnipping)
                                                        App.SetSnipDeletingTimer(
                                                            Settings.Default.TimeBeforeDeletingPicture);
                                                    else if (App.TimeBeforeDeletingSpan != null)
                                                    {
                                                        App.TimeBeforeDeletingSpan.Stop();
                                                        App.TimeBeforeDeletingSpan.Dispose();
                                                    }
                                                            
                                                        
                                                   
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
                SnipName = Settings.Default.SnipName;
                SnipLocation = Settings.Default.SnipLocation;
                AllowSnipOverwriting = Settings.Default.AllowSnipOverwriting;
                AllowDeletingPictureAfterSnipping = Settings.Default.AllowDeletingPictureAfterSnipping;
                TimeBeforeDeletingPicture = Settings.Default.TimeBeforeDeletingPicture;
                IsCopyImageToClipBoard = Settings.Default.IsCopyImageToClipBoard;
                IsCopyURIToClipboard = Settings.Default.IsCopyUriToClipboard;
            }

        #endregion

        #region Fields

        private string _snipName;
        private string _snipLocation;
        private bool _allowSnipOverwriting;
        private bool _allowDeletingPictureAfterSnipping;
        private double _timeBeforeDeletingPicture;
        private bool _isCopyImageToClipBoard;
        private bool _isCopyURIToClipboard;

        #endregion
    }
}