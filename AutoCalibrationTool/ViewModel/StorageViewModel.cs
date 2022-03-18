using GalaSoft.MvvmLight;
using System.Windows;

namespace AutoCalibrationTool.ViewModel
{
    public class StorageViewModel : ViewModelBase
    {
        #region Fields
        private string storageLocation;
        #endregion

        #region Properties
        public string StorageLocation
        {
            get => storageLocation;
            set
            {
                Set(ref storageLocation, value);
                UpdateButtonStatus();
            }
        }

        public bool ExportButtonEnabled => !string.IsNullOrEmpty(storageLocation);

        public Visibility TestButtonVisible { get; private set; } = Visibility.Collapsed;
        #endregion

        #region Methods
        public void UpdateButtonStatus()
        {
            RaisePropertyChanged(nameof(ExportButtonEnabled));
        }

        public void SetTestButtonVisibility(Visibility visibility)
        {
            TestButtonVisible = visibility;
            RaisePropertyChanged(nameof(TestButtonVisible));
        }
        #endregion
    }
}
