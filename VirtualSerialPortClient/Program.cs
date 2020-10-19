using System.IO.Ports;
using System;
using CommonLib.Extensions;
using System.Threading;
using System.Configuration;

namespace VirtualSerialPortClient
{
    /// <summary>
    /// 配合使用Virtual Serial Port Driver工具进行测试
    /// </summary>
    class Program
    {
        private static SerialPort port;
        private static string portName = "COM2";

        static void Main(string[] args)
        {
            portName = ConfigurationManager.AppSettings["Port"];
            if(!string.IsNullOrWhiteSpace(portName))
            {
                port = new SerialPort();
                port.PortName = portName;
                port.BaudRate = 9600;
                port.DtrEnable = true;
                port.DataReceived += Port_DataReceived;
                port.Open();
                Console.ReadKey();
                port.Close();
                port.Dispose();
            }
            else
            {
                Console.WriteLine("Configuration key is missing: [Port]");
                Console.ReadKey();
            }
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = port.BytesToRead;
            byte[] buffer = new byte[count];
            port.Read(buffer, 0, count);
            Console.WriteLine($"{portName} received: {buffer.ToHexString()}");
            Thread.Sleep(10);

            if (buffer.Length != 8)
                return;

            byte[] dataToSend;
            if (buffer[3] == 0x16 && buffer[4] == 0x00 && buffer[5] == 0x0B)
                dataToSend = GetFlow(buffer[0]);
            else if (buffer[3] == 0x03 && buffer[4] == 0x00 && buffer[5] == 0x10)
                dataToSend = GetBaseInfo(buffer[0]);
            else dataToSend = null;

            if (dataToSend != null)
            {
                port.Write(dataToSend, 0, dataToSend.Length);
                Console.WriteLine($"{portName} send: {dataToSend.ToHexString()}");
            }
        }

        private static byte[] GetBaseInfo(byte addr)
        {
            byte[] ret = new byte[37];
            ret[0] = addr;
            ret[1] = 0x03;
            ret[2] = 0x03;
            ret[3] = 0x00;
            ret[4] = 0x04;
            ret[5] = 0x00;
            ret[6] = 0xC8;
            ret[7] = 0x00;
            ret[8] = 0x0A;
            for (int i = 9; i <= 23; i++)
                ret[i] = 0x00;
            for (int i = 23; i <= 34; i++)
                ret[i] = 0x00;
            byte[] crc = ret.GetCRC16(ret.Length - 2);
            ret[35] = crc[0];
            ret[36] = crc[1];
            return ret;
        }

        private static byte[] GetFlow(byte addr)
        {
            //addr 0x03 0x16 FLOW1 FLOW2 FLOW3 FLOW4
            //ACCMULATE1 ACCMULATE2 ACCMULATE3 ACCMULATE4 ACCMULATE5 ACCMULATE6 ACCMULATE7 ACCMULATE8
            //UNIT1 UNIT2 DAY1 DAY2 HOUR1 HOUR2 MIN1 MIN2 SEC1 SEC2
            //CRCL CRCH
            byte[] ret = new byte[27];
            ret[0] = addr;
            ret[1] = 0x03;
            ret[2] = 0x16;
            //52.32 * 100 = 5232 = 0x1600
            ret[3] = 0x00;
            ret[4] = 0x00;
            ret[5] = 0x16;
            ret[6] = 0x00;
            //52.32 * 1000 = 52320 = 0xDC00
            for (int i = 7; i <= 12; i++)
                ret[i] = 0x00;
            ret[13] = 0xDC;
            ret[14] = 0x00;

            ret[15] = 0x00;
            ret[16] = 0x01;

            for (int i = 17; i <= 24; i++)
                ret[i] = 0x00;

            byte[] crc = ret.GetCRC16(ret.Length - 2);
            ret[25] = crc[0];
            ret[26] = crc[1];
            return ret;
        }
    }
}
