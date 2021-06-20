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
                storageLocation = value;
                RaisePropertyChanged();
            }
        }
        public bool ExportButtonEnabled => ViewModelLocator.Main.Mode == Models.CalibrationMode.Stop;
        public Visibility TestButtonVisible { get; private set; } = Visibility.Collapsed;
        #endregion

        #region Methods
        public void UpdateButtonStatus()
        {
            RaisePropertyChanged(nameof(ExportButtonEnabled));
        }

        public void SetTestButtonToVisible()
        {
            TestButtonVisible = Visibility.Visible;
            RaisePropertyChanged(nameof(TestButtonVisible));
        }
        #endregion
    }
}
