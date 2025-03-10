﻿using CommonLib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace CommonLib.Communication.Serial
{
    /// <summary>
    /// 在SerialPort类的基础上实现了数据的自动收取，后续使用仅编写数据处理逻辑即可。
    /// </summary>
    public sealed class AdvancedSerialPort : SerialPort
    {
        public AdvancedSerialPort() : base()
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }

        public AdvancedSerialPort(string portName) : base(portName) 
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }
        public AdvancedSerialPort(IContainer container) : base(container) 
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }
        public AdvancedSerialPort(string portName, int baudRate):base(portName, baudRate) 
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }
        public AdvancedSerialPort(string portName, int baudRate, Parity parity) : 
            base(portName, baudRate, parity) 
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }
        public AdvancedSerialPort(string portName, int baudRate, Parity parity, int dataBits) : 
            base(portName, baudRate, parity, dataBits) 
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }
        public AdvancedSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits):
            base(portName, baudRate, parity, dataBits, stopBits)
        {
            this.DataReceived += AdvancedSerialPort_DataReceived;
        }

        /// <summary>
        /// 使用此串口类仅需实现此属性即可
        /// </summary>
        public Action<byte[]> ReceivedDataHandler { get; set; }

        private readonly object syncObject = new object();
        private void AdvancedSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (syncObject)
            {
                List<byte> receivedBytes = new List<byte>();
                try
                {
                    while (this.BytesToRead > 0)
                    {
                        int count = this.BytesToRead;
                        byte[] buffer = new byte[count];
                        if (this.Read(buffer, 0, count) > 0)
                        {
                            receivedBytes.AddRange(buffer);
                        }
                        Thread.Sleep(10);
                    }
                    ReceivedDataHandler?.Invoke(receivedBytes.ToArray());
                }
                finally
                {
                    receivedBytes.Clear();
                }
            }
        }

        public new static List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public static List<Parity> GetParities()
            => EnumExtension.GetEnumList<Parity>();

        public static List<StopBits> GetStopBits()
            => EnumExtension.GetEnumList<StopBits>();
    }
}
