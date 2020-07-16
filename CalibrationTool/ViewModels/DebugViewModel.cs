using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationTool.ViewModels
{
    public class DebugViewModel : BindableBase
    {
        public RelayCommand DebugCommand { get; set; }
    }
}
