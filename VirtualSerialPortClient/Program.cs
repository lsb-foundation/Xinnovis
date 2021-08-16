using System.IO.Ports;
using System;
using CommonLib.Extensions;
using System.Threading;
using System.Configuration;
using System.Text;
using System.Collections.Generic;

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
                port = new SerialPort
                {
                    PortName = portName,
                    BaudRate = 9600,
                    DtrEnable = true
                };
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
            int count = 0;
            List<byte> bufferList = new List<byte>();
            while ((count = port.BytesToRead) > 0)
            {
                byte[] bytes = new byte[count];
                _ = port.Read(bytes, 0, count);
                bufferList.AddRange(bytes);
                Thread.Sleep(10);
            }

            byte[] buffer = bufferList.ToArray();
            Console.WriteLine($"{portName} received: {buffer.ToHexString()}");
            //if (buffer.Length != 8)
            //    return;

            byte[] dataToSend;
            if (buffer[3] == 0x16 && buffer[4] == 0x00 && buffer[5] == 0x0B)
                dataToSend = GetFlow(buffer[0]);
            else if (buffer[3] == 0x03 && buffer[4] == 0x00 && buffer[5] == 0x10)
                dataToSend = GetBaseInfo(buffer[0]);
            else if (Encoding.Default.GetString(buffer) == "DEBUG!")
                dataToSend = GetDebugData();
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
            ret[4] = 0x07;
            ret[5] = 0x01;
            ret[6] = 0xF4;
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
            Random random = new Random();
            float flow = ((float)random.NextDouble() * 50 + 400) * 100;
            byte[] flowBytes = BitConverter.GetBytes((int)flow);
            //52.32 * 100 = 5232 = 0x1600
            ret[3] = flowBytes[3];
            ret[4] = flowBytes[2];
            ret[5] = flowBytes[1];
            ret[6] = flowBytes[0];
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

        private static byte[] GetDebugData()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"SN:TESTSN001\r\n")
                .Append($"Type of GAS:O2\r\n")
                .Append($"Range:100\r\n")
                .Append($"UNIT:SCCM\r\n")
                .Append($"modbus addr:1\r\n")
                .Append($"modbus baud:9600\r\n")
                .Append($"K of 4-20mA:2.98\r\n")
                .Append($"D of 4-20mA:19.90\r\n")
                .Append($"K of 1-5V:8.67\r\n")
                .Append($"D of 1-5V:29.77\r\n")
                .Append($"T of cali flow:46\r\n")
                .Append($"T of now:23\r\n")
                .Append($"GAS FACTOR:18.54\r\n")
                .Append($"P:6.87\r\n")
                .Append($"I:8.78\r\n")
                .Append($"D:6.54\r\n")
                .Append($"Zero:7.34\r\n");
            return Encoding.Default.GetBytes(builder.ToString());
        }
    }
}
