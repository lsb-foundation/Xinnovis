﻿using CommonLib.Extensions;
using MFCSoftware.Utils;
using CommonLib.Utils;
using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using MFCSoftware.Models;
using System.IO;

namespace MFCSoftware.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel mainVm;
        private readonly CancellationTokenSource _cancel = new();
        private readonly LinkedList<ChannelUserControl> _controlList = new();

        private bool _isSending = true;
        
        public MainWindow()
        {
            InitializeComponent();
            mainVm = DataContext as MainWindowViewModel;
            
            StartLoopTask();
        }

        private void OpenSetSerialPortWindow(object sender, EventArgs e)
        {
            SetSerialPortWindow window = new()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ShowInTaskbar = false
            };
            window.ShowDialog();
        }

        private void OpenSetAddressWindow(object sender, EventArgs e)
        {
            SetAddressWindow window = new()
            {
                Owner = this,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.ShowDialog();
        }

        private void StartLoopTask()
        {
            Task.Factory.StartNew(async () =>
            {
                LinkedListNode<ChannelUserControl> currentNode = null;
                while (!_cancel.IsCancellationRequested)
                {
                    if (_controlList.Count > 0)
                    {
                        if (currentNode is null) currentNode = _controlList.First;
                        else if (currentNode == _controlList.Last) currentNode = _controlList.First;
                        else currentNode = currentNode.Next;

                        if (currentNode?.Value != null)
                        {
                            var channel = currentNode.Value;
                            await SendResolveAsync(channel, channel.ReadFlowBytes);
                        }
                    }
                }
            }, _cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void AddChannelButton_Click(object sender, RoutedEventArgs e)
        {
            AddChannelWindow window = new()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ShowInTaskbar = false
            };

            window.ShowDialog();

            if (!window.IsAddressSetted) return;

            bool addrExists = _controlList.Any(c => c.Address == window.Address);
            if (!addrExists)
            {
                AddChannel(window.Address);
            }
            else mainVm.ShowMessage("地址重复，请重新添加。");
        }

        private async void AddChannel(int address)
        {
            ChannelUserControl channel = new(address);
            channel.ChannelClosed += ChannelControl_ControlWasRemoved;
            channel.SingleCommandSended += async (chn, cmd) => await SendResolveAsync(chn, cmd);

            ChannelGrid.Children.Add(channel);
            _controlList.AddLast(channel);
            mainVm.ChannelCount++;
            await SendResolveAsync(channel, channel.ReadBaseInfoBytes); //添加通道后读取基本信息
            await SendResolveAsync(channel, channel.ReadVersion); //添加通道后读取版本信息
        }

        private async Task SendResolveAsync(ChannelUserControl channel, SerialCommand<byte[]> command)
        {
            try
            {
                LoggerHelper.WriteLog("[Send]" + command.Command.ToHexString());
                var data = await SerialPortInstance.GetResponseAsync(command);
                channel.ResolveData(data, command.Type);
            }
            catch (TimeoutException)
            {
                channel.WhenTimeOut();
            }
            catch (Exception e)
            {
                mainVm.ShowMessage("发生异常: " + e.Message);
                LoggerHelper.WriteLog(e.Message, e);
            }
        }

        private void ChannelControl_ControlWasRemoved(ChannelUserControl channel)
        {
            _controlList.Remove(channel);
            ChannelGrid.Children.Remove(channel);
            mainVm.ChannelCount--;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string port = await SqliteHelper.GetSettings("PortName");
            string baudrate = await SqliteHelper.GetSettings("BaudRate");
            string seriesPointNumber = await SqliteHelper.GetSettings("SeriesPointNumber");

            if (!string.IsNullOrEmpty(port))
            {
                ViewModelLocator.SetSerialViewModel.PortName = port;
            }
            ViewModelLocator.SetSerialViewModel.BaudRate = string.IsNullOrEmpty(baudrate) ? 9600 : int.Parse(baudrate);
            ViewModelLocator.SetSerialViewModel.SeriesPointNumber = string.IsNullOrEmpty(seriesPointNumber) ? 50 : int.Parse(seriesPointNumber);
            int.TryParse(ConfigurationManager.AppSettings["ComInterval"], out SerialPortInstance.WaitTime);

            LoadChannels();
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            SaveChannels();

            _cancel.Cancel();
            ChannelGrid.Children.Clear();
            _controlList.Clear();

            await SqliteHelper.UpdateSettings("PortName", ViewModelLocator.SetSerialViewModel.PortName);
            await SqliteHelper.UpdateSettings("BaudRate", ViewModelLocator.SetSerialViewModel.BaudRate.ToString());
            await SqliteHelper.UpdateSettings("SeriesPointNumber", ViewModelLocator.SetSerialViewModel.SeriesPointNumber.ToString());
        }

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSending)
            {
                await ComSharingService.Semaphore.WaitAsync();
                var comInstance = SerialPortInstance.GetSerialPortInstance();
                if (comInstance.IsOpen)
                {
                    comInstance.Close();
                }
            }
            else
            {
                ComSharingService.Semaphore.Release();
            }
            
            _isSending = !_isSending;
            (sender as Button).Content = _isSending ? "暂停" : "开始";
        }

        private async void SaveChannels()
        {
            ChannelSaveModel model = new() { Channels = new List<int>() };
            foreach (var control in _controlList)
            {
                model.Channels.Add(control.Address);
            }

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./channels.json");
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            using var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);
        }

        private async void LoadChannels()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./channels.json");
            if (!File.Exists(file)) return;

            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            ChannelSaveModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelSaveModel>(json);

            foreach (int address in model.Channels)
            {
                AddChannel(address);
            }
        }
    }
}
