using MFCSoftwareForCUP.ViewModels;
using System.Windows;

namespace MFCSoftwareForCUP.Views
{
    /// <summary>
    /// ChannelEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelEditor : Window
    {
        private readonly ChannelViewModel _channel;

        public ChannelEditor(ChannelViewModel channelViewModel)
        {
            InitializeComponent();
            _channel = channelViewModel;
            Title = $"编辑 - 设备{_channel.Address}";
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string floor = FloorTextBox.Text.Trim();
            string room = RoomTextBox.Text.Trim();
            string gasType = GasTypeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(floor))
            {
                TipsLabel.Content = "楼层不能为空！";
                return;
            }
            else if (string.IsNullOrEmpty(room))
            {
                TipsLabel.Content = "房间号不能为空！";
                return;
            }
            else if (string.IsNullOrEmpty(gasType))
            {
                TipsLabel.Content = "气体类型不能为空！";
                return;
            }

            _channel.Floor = floor;
            _channel.Room = room;
            _channel.GasType = gasType;
            Close();
        }
    }
}
