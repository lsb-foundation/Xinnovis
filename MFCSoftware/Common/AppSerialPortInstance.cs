using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using CommonLib.Communication;
using CommonLib.Communication.Serial;
using MFCSoftware.Models;

namespace MFCSoftware.Common
{
    public static class AppSerialPortInstance
    {
        private static SerialPort com;
        private static ConcurrentQueue<byte> buffer = new ConcurrentQueue<byte>();
        private static SerialCommand<byte[]> currentCommand = null;

        static AppSerialPortInstance()
        {
            com = new SerialPort();
            com.DtrEnable = true;
            com.DataReceived += Sp_DataReceived;
        }

        private static void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = com.BytesToRead;
            byte[] buff = new byte[count];
            com.Read(buff, 0, count);
            foreach (var item in buff)
                buffer.Enqueue(item);
        }

        public static SerialPort GetSerialPortInstance()
        {
            if (com == null)
                com = new SerialPort();
            return com;
        }

        public static void Send(SerialCommand<byte[]> command)
        {
            try
            {
                if (!com.IsOpen)
                    com.Open();
                com.Write(command.Command, 0, command.Command.Length);
                currentCommand = command;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public async static Task<byte[]> GetResponseBytes()
        {
            using (System.Timers.Timer timer = new System.Timers.Timer(100))
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                timer.AutoReset = false;
                timer.Elapsed += (s, e) => cts.Cancel();

                List<byte> buff = new List<byte>();
                await Task.Run(() =>
                {
                    timer.Start();
                    while (buff.Count < currentCommand.ResponseLength)
                    {
                        if (cts.IsCancellationRequested)
                            break;
                        if (buffer.TryDequeue(out byte data))
                            buff.Add(data);
                    }
                }, cts.Token);

                if (cts.IsCancellationRequested)
                {
                    ClearBuffer();
                    throw new TimeoutException();
                }
                else
                {
                    timer.Stop();
                    return buff.ToArray();
                }
            }  
        }

        private async static void ClearBuffer()
        {
            using (System.Timers.Timer timer = new System.Timers.Timer(100))
            using(CancellationTokenSource cts = new CancellationTokenSource())
            {
                timer.AutoReset = false;
                timer.Elapsed += (s, e) => cts.Cancel();
                await Task.Run(() =>
                {
                    timer.Start();
                    while (com.BytesToRead > 0)
                    {
                        if (cts.IsCancellationRequested)
                            break;
                        com.ReadExisting();
                    }
                }, cts.Token);
                timer.Stop();
            }    
        }
    }
}
