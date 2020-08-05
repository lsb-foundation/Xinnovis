using LiveCharts.Defaults;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SerialDataDisplay
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;
        private SerialPort serial;
        private bool sending;
        //private ObservableDataSource<Point> pointDataSource;
        //private int i = 0;

        public MainWindow()
        {
            this.Loaded += MainWindow_Loaded;
            InitializeComponent();
            vm = new MainWindowViewModel();
            serial = vm.Serial;
            DataContext = vm;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //pointDataSource = new ObservableDataSource<Point>();
            //plotter.AddLineGraph(pointDataSource, Colors.Green, 2);
            //plotter.Viewport.FitToView();

            serial.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string asciis = serial.ReadLine();
            if (float.TryParse(asciis, out float data))
                //UpdateDataSource(data);
                UpdateViewModel(data);
        }

        //private void UpdateDataSource(float number)
        //{
        //    Task.Run(() =>
        //    {
        //        Point point = new Point(++i, number);
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            pointDataSource.AppendAsync(this.Dispatcher, point);
        //            //if (i > 20) plotter.Visible = new Rect(i - 20, plotter.Visible.Y, plotter.Visible.Width, plotter.Visible.Height);
        //        });
        //    });
        //}

        private void UpdateViewModel(float number)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    vm.SeriesCollection[0].Values.Add(new ObservableValue(number));
                    vm.SeriesCollection[0].Values.RemoveAt(0);
                    vm.CurrentVolte = number;
                });
            });
        }

        private bool Send(byte[] data)
        {
            try
            {
                if (!serial.IsOpen)
                    serial.Open();
                
                serial.Write(data, 0, data.Length);
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("串口连接失败，请检查！\n" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[] { 0xCA };
            if (Send(data))
                sending = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[] { 0xED };
            if (Send(data))
                sending = false;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (!sending) return;
            btnStop_Click(this, null);
        }
    }
}
