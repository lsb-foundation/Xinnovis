using System.IO.Ports;
using System;
using CommonLib.Extensions;
using System.Threading;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace VirtualSerialPortClient
{
    /// <summary>
    /// 配合使用Virtual Serial Port Driver工具进行测试
    /// </summary>
    class Program
    {
        private static SerialPort port;
        private static string portName = "COM2";
        private static Random _random = new Random(Guid.NewGuid().GetHashCode());

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
            int count;
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
            if (buffer[3] == 0x14 && buffer[4] == 0x00 && buffer[5] == 0x0D)
                dataToSend = GetFlowWithTemperature(buffer[0]);
            else if (buffer[3] == 0x03 && buffer[4] == 0x00 && buffer[5] == 0x10)
                dataToSend = GetBaseInfo(buffer[0]);
            else if (buffer[3] == 0x28 && buffer[4] == 0x00 && buffer[5] == 0x0A)
                dataToSend = GetVersion(buffer[0]);
            else if (Encoding.Default.GetString(buffer) == "DEBUG!")
                dataToSend = GetDebugData();
            else if (Encoding.Default.GetString(buffer) == "INCUBE_START!")
            {
                IncubeStart();
                return;
            }
            else dataToSend = null;

            if (dataToSend != null)
            {
                Send(dataToSend);
            }

            if (Encoding.Default.GetString(buffer) == "EXPORT!")
            {
                for(int index = 0; index < 100; ++index)
                {
                    dataToSend = GetHandHeldMeterExporterData();
                    Send(dataToSend);
                    Thread.Sleep(100);
                }
                dataToSend = Encoding.Default.GetBytes("Export complete\r\n");
                Send(dataToSend);
            }
        }

        private static void Send(byte[] dataToSend)
        {
            port.Write(dataToSend, 0, dataToSend.Length);
            Console.WriteLine($"{portName} send: {dataToSend.ToHexString()}");
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

        private static byte[] GetFlowWithTemperature(byte addr)
        {
            byte[] ret = new byte[32];
            ret[0] = addr;
            ret[1] = 0x03;
            ret[2] = 0x1A;
            ret[3] = 0x00;
            ret[4] = 0x00;
            ret[5] = 0x80;
            ret[6] = 0x00;
            Random random = new Random();
            float flow = ((float)random.NextDouble() * 50 + 400) * 100;
            byte[] flowBytes = BitConverter.GetBytes((int)flow);
            //52.32 * 100 = 5232 = 0x1600
            ret[7] = flowBytes[3];
            ret[8] = flowBytes[2];
            ret[9] = flowBytes[1];
            ret[10] = flowBytes[0];
            //52.32 * 1000 = 52320 = 0xDC00
            for (int i = 11; i <= 18; i++)
                ret[i] = 0x00;
            ret[19] = 0xDC;
            ret[20] = 0x00;

            for (int i = 21; i <= 29; i++)
                ret[i] = 0x00;
            byte[] crc = ret.GetCRC16(ret.Length - 2);
            ret[30] = crc[0];
            ret[31] = crc[1];
            return ret;
        }
        
        private static byte[] GetVersion(byte addr)
        {
            var bytes = new List<byte>();
            bytes.Add(addr);
            bytes.AddRange(new byte[] { 0x03, 0x14, 0x4D, 0x46, 0x43, 0x2D, 0x45, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x31, 0x2E, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            byte[] crc = bytes.ToArray().GetCRC16ByDefault();
            bytes.AddRange(crc);
            return bytes.ToArray();
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

        private static byte[] GetHandHeldMeterExporterData()
        {
            int floor = _random.Next(1, 10);
            int department = _random.Next(1, 10);
            int room = _random.Next(1, 50);
            int bedNumber = _random.Next(1, 5);
            float flow = 20.0f + (float)_random.NextDouble() * 10;
            float pressure = 50.0f + (float)_random.NextDouble() * 20;
            float temperature = 20.0f + (float)_random.NextDouble() * 10;
            float humidity = 10.0f + (float)_random.NextDouble() * 5;
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string time = DateTime.Now.ToString("HH:mm:ss");
            string text = $"A{floor};B{department};C{room};D{bedNumber};E{flow};F{pressure};G{temperature};H{humidity};{date},{time}\n";
            return Encoding.Default.GetBytes(text);
        }

        private async static void IncubeStart()
        {
            var incubeFile = Path.Combine(AppContext.BaseDirectory, "datas\\室温一次数据.txt");
            using var stream = new FileStream(incubeFile, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            while (await reader.ReadLineAsync() is string line)
            {
                if (!line.EndsWith("\r\n")) line += "\r\n";
                byte[] data = Encoding.Default.GetBytes(line);
                Send(data);
                Thread.Sleep(30);
            }
        }
    }
}
