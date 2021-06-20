using GalaSoft.MvvmLight;

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
        #endregion

        #region Methods
        public void UpdateButtonStatus()
        {
            RaisePropertyChanged(nameof(ExportButtonEnabled));
        }
        #endregion
    }
}
