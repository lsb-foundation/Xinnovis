using CommonLib.Extensions;
using CommonLib.Mvvm;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommonLib.Models;

namespace MFCSoftware.ViewModels
{
    public class ChannelUserControlViewModel:BindableBase
    {
        private int x;
        public ChannelUserControlViewModel()
        {
            PointDataSource = new ObservableDataSource<Point>();
        }

        public ObservableDataSource<Point> PointDataSource { get; }

        public int Address { get; private set; }
        public string SN { get; private set; }
        public GasTypeCode GasType { get; private set; }
        public int Range { get; private set; }
        public UnitCode Unit { get; private set; }
        public float CurrentFlow { get; private set; }
        public float FlowToSet { get; set; }
        public float ValveValue { get; set; }

        public List<string> Units { get; } = new List<string>() { "SCCM", "UCCM", "CCM", "SLM" };

        public byte[] ReadFlowBytes { get; private set; }
        public byte[] ReadBaseInfoBytes { get; private set; }

        public Action<Point> AppendPoint { get; set; }

        /// <summary>
        /// 根据一级解析出的地址回传到此处进行二次数据解析。
        /// </summary>
        public void SencondResolve(byte[] data)
        {
            Task.Run(() =>
            {
                if (data.Length == 37)
                    ReadBaseInformation(data);
                else if (data.Length == 7)
                {
                    //暂时先按照采集流量处理
                    ReadFlow(data);
                }
            });
        }

        public void SetAddress(int addr)
        {
            Address = addr;
            RaiseProperty(nameof(Address));
            SetReadFlowBytes();
            SetReadBaseInfoBytes();
        }

        private void SetReadFlowBytes()
        {
            //addr 0x03 0x00 0x16 0x00 0x01 CRCL CRCH
            byte addr = Convert.ToByte(Address);
            List<byte> bytes = new List<byte>();
            bytes.Add(addr);
            bytes.AddRange(new byte[] { 0x03, 0x00, 0x16, 0x00, 0x01 });
            var crc = bytes.ToArray().GetCRC16(bytes.Count);
            bytes.AddRange(crc);
            ReadFlowBytes = bytes.ToArray();
        }

        private void SetReadBaseInfoBytes()
        {
            //addr 0x03 0x00 0x03 0x00 0x10 CRCL CRCH
            byte addr = Convert.ToByte(Address);
            List<byte> bytes = new List<byte>();
            bytes.Add(addr);
            bytes.AddRange(new byte[] { 0x03, 0x00, 0x03, 0x00, 0x10 });
            var crc = bytes.ToArray().GetCRC16(bytes.Count);
            bytes.AddRange(crc);
            ReadBaseInfoBytes = bytes.ToArray();
        }

        private void ReadBaseInformation(byte[] data)
        {
            byte[] gasTypeBytes = GetChildArray(data, 3, 2);
            int gasCode = gasTypeBytes.ToInt32(0, 2);
            GasType = GasTypeCode.GetGasTypeCodes().FirstOrDefault(c => c.Code == gasCode);
            RaiseProperty(nameof(GasType));

            byte[] rangeBytes = GetChildArray(data, 5, 2);
            int rangeInt = rangeBytes.ToInt32(0, 2);
            Range = rangeInt;
            RaiseProperty(nameof(Range));

            byte[] unitBytes = GetChildArray(data, 7, 2);
            int unitCode = unitBytes.ToInt32(0, 2);
            Unit = UnitCode.GetUnitCodes().FirstOrDefault(c => c.Code == unitCode);
            RaiseProperty(nameof(Unit));

            byte[] snBytes = GetChildArray(data, 23, 12);
            SN = snBytes.ToHexString();
            RaiseProperty(nameof(SN));
        }

        private void ReadFlow(byte[] data)
        {
            //addr 0x03 length FlowH FlowL CRCL CRCH
            byte[] flow = GetChildArray(data, 3, 2);
            int intFlow = flow.ToInt32(0, 2);
            CurrentFlow = intFlow / 100.0f;
            RaiseProperty(nameof(CurrentFlow));

            Point point = new Point(++x, CurrentFlow);
            AppendPoint?.Invoke(point);
        }

        private byte[] GetChildArray(byte[] array, int index, int length)
        {
            byte[] ret = new byte[length];
            Array.Copy(array, index, ret, 0, length);
            return ret;
        }
    }
}
