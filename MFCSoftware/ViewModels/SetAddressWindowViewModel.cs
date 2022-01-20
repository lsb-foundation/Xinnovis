using CommonLib.Communication.Serial;
using MFCSoftware.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFCSoftware.ViewModels
{
    public class SetAddressWindowViewModel : ViewModelBase
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
            set => Set(ref _readerAddress, value);
        }
        private uint _writerAddress;
        public uint WriterAddress
        {
            get => _writerAddress;
            set => Set(ref _writerAddress, value);
        }

        public List<BaudRateCode> BaudRateCodes { get; private set; }

        private BaudRateCode _baudRateCode;
        public BaudRateCode BaudRateCode
        {
            get => _baudRateCode;
            set => Set(ref _baudRateCode, value);
        }

        private bool _enable = true;
        public bool Enable
        {
            get => _enable;
            set => Set(ref _enable, value);
        }

        public SerialCommand<byte[]> ReadAddressBytes
        {
            get
            {
                byte[] command = new byte[] { 0xFE, 0x03, 0x00, 0x00, 0x00, 0x01, 0x90, 0x05 };
                return new SerialCommand<byte[]>(command, SerialCommandType.ReadAddress, 7);
            }
        }

        public SerialCommand<byte[]> WriteAddressBytes
        {
            get
            {
                byte addr = Convert.ToByte(_writerAddress);
                byte[] header = { 0xFE, 0x06, 0x00, 0x00, 0x00 };
                return new SerialCommandBuilder(SerialCommandType.WriteAddress)
                    .AppendBytes(header)
                    .AppendBytes(addr)
                    .AppendCrc16()
                    .WithResponseLength(8)
                    .Build();
            }
        }

        public SerialCommand<byte[]> SetBaudRateBytes
        {
            get
            {
                byte[] header = { 0xFE, 0x06, 0x00, 0x01, 0x00 };
                byte baudcode = Convert.ToByte(_baudRateCode.Code);
                return new SerialCommandBuilder(SerialCommandType.SetBaudRate)
                    .AppendBytes(header)
                    .AppendBytes(baudcode)
                    .AppendCrc16()
                    .WithResponseLength(8)
                    .Build();
            }
        }
    }
}
