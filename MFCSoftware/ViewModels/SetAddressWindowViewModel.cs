﻿using CommonLib.Communication.Serial;
using CommonLib.Mvvm;
using MFCSoftware.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                //0xFE 0x06 0x00 0x00 0x00 addr CRCL CRCH
                byte addr = Convert.ToByte(_writerAddress);
                byte[] header = { 0xFE, 0x06, 0x00, 0x00, 0x00 };
                return new SerialCommandBuilder(SerialCommandType.WriteAddress)
                    .AppendBytes(header)
                    .AppendBytes(addr)
                    .AppendCrc16()
                    .ToSerialCommand(7);
            }
        }

        public SerialCommand<byte[]> SetBaudRateBytes
        {
            get
            {
                //0xFE 0x06 0x00 0x01 0x00 baudcode CRCL CRCH
                byte[] header = { 0xFE, 0x06, 0x00, 0x01, 0x00 };
                byte baudcode = Convert.ToByte(_baudRateCode.Code);
                return new SerialCommandBuilder(SerialCommandType.SetBaudRate)
                    .AppendBytes(header)
                    .AppendBytes(baudcode)
                    .AppendCrc16()
                    .ToSerialCommand(7);
            }
        }
    }
}
