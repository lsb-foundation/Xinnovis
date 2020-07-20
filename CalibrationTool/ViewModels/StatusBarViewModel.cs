using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalibrationTool.ViewModels
{
    public class StatusBarViewModel : BindableBase
    {
        //private Dictionary<AppStatus, string> statusDict = new Dictionary<AppStatus, string>()
        //{
        //    {AppStatus.Ready,"准备就绪" },
        //    {AppStatus.SerialPort_Opened,"端口状态：打开" },
        //    {AppStatus.SerialPort_Closed,"端口状态：关闭" },
        //    {AppStatus.Running,"正在运行" },
        //    {AppStatus.Error,"错误" }
        //};

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private CancellationTokenSource _source;
        private Task _task;

        public StatusBarViewModel()
        {
            _source = new CancellationTokenSource();
        }

        public void ShowStatus(string message)
        {
            if (_task != null && !_task.IsCompleted)
                _source.Cancel();

            _task = Task.Run(async () =>
            {
                _status = message;
                RaiseProperty(nameof(Status));
                await Task.Delay(3000);
                _status = string.Empty;
                RaiseProperty(nameof(Status));
            }, _source.Token);
        }
    }

    //public enum AppStatus
    //{
    //    Ready,      //准备就绪
    //    SerialPort_Opened,      //端口状态：打开
    //    SerialPort_Closed,      //端口状态：关闭
    //    Running,    //正在运行
    //    Error,      //错误
    //}
}
