using LiveCharts.Defaults;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using OfficeOpenXml;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    vm.InsertValue(number);
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

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var values = vm.QueryValues();
            if(values.Count > 0)
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Excel文件|*.xlsx;*.xls";
                dialog.Title = "保存Excel文件";
                if ((bool)dialog.ShowDialog())
                {
                    if (!string.IsNullOrEmpty(dialog.FileName))
                    {
                        ExportValuesToExcel(dialog.FileName, values);
                    }
                }
            }
            else
            {
                MessageBox.Show("未查询到数据。");
            }
        }

        private async void ExportValuesToExcel(string fileName, List<TableValue> values)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet sheet;
                    if (package.Workbook.Worksheets.Any(e => e.Name == "Sheet1"))
                        sheet = package.Workbook.Worksheets.FirstOrDefault(e => e.Name == "Sheet1");
                    else sheet = package.Workbook.Worksheets.Add("Sheet1");

                    sheet.SetValue(1, 1, "collect_time");
                    sheet.SetValue(1, 2, "value");

                    for (int index = 0; index < values.Count; index++)
                    {
                        sheet.SetValue(index + 2, 1, values[index].Time.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        sheet.SetValue(index + 2, 2, values[index].Value);
                    }
                    await package.SaveAsync();
                    sheet.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("导出Excel出错：\n" + e.Message);
            }
        }
    }
}
