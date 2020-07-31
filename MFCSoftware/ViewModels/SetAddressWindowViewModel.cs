using CommonLib.Communication.Serial;
using CommonLib.Extensions;
using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFCSoftware.ViewModels
{
    public class SetAddressWindowViewModel:BindableBase
    {
        public SetAddressWindowViewModel()
        {
            BaudRateCodes = BaudRateCode.GetBaudRateCodes();
            BaudRateCode = BaudRateCodes.FirstOrDefault(c => c.BaudRate == 9600);
        }

        private uint _readerAddress;
        public uint ReaderAddress
        {
            get => _readerAddress;
            set => SetProperty(ref _readerAddress, value);
        }
        private uint _writerAddress;
        public uint WriterAddress
        {
            get => _writerAddress;
            set => SetProperty(ref _writerAddress, value);
        }

        public List<BaudRateCode> BaudRateCodes { get; private set; }

        private BaudRateCode _baudRateCode;
        public BaudRateCode BaudRateCode
        {
            get => _baudRateCode;
            set => SetProperty(ref _baudRateCode, value);
        }

        private bool _enable = true;
        public bool Enable
        {
            get => _enable;
            set => SetProperty(ref _enable, value);
        }

        public byte[] ReadAddressBytes { get; } = new byte[] { 0xFE, 0x03, 0x00, 0x00, 0x00, 0x01, 0x90, 0x05 };

        public byte[] WriteAddressBytes
        {
            get
            {
                //0xFE 0x06 0x00 0x00 0x00 addr CRCL CRCH
                byte addr = Convert.ToByte(_writerAddress);
                byte[] header = { 0xFE, 0x06, 0x00, 0x00, 0x00 };
                return GetBytesCommand(header, addr);
            }
        }

        public byte[] SetBaudRateBytes
        {
            get
            {
                //0xFE 0x06 0x00 0x01 0x00 baudcode CRCL CRCH
                byte[] header = { 0xFE, 0x06, 0x00, 0x01, 0x00 };
                byte baudcode = Convert.ToByte(_baudRateCode.Code);
                return GetBytesCommand(header, baudcode);
            }
        }

        private byte[] GetBytesCommand(byte[] header, params byte[] data)
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(header);
            ret.AddRange(data);
            byte[] crcCode = ret.ToArray().GetCRC16(ret.Count);
            ret.AddRange(crcCode);
            return ret.ToArray();
        }
    }
}
