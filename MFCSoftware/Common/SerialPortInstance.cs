using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using MFCSoftware.Models;

namespace MFCSoftware.Common
{
    public static class SerialPortInstance
    {
        private static SerialPort com;
        private static readonly ConcurrentQueue<byte> buffer = new ConcurrentQueue<byte>();
        private static SerialCommand<byte[]> currentCommand = null;
        //private static bool timeOut;
        private const int waitTime = 100;

        //public static event Action<byte[]> DataHandler;

        static SerialPortInstance()
        {
            com = new SerialPort();
            com.DtrEnable = true;
            //com.DataReceived += Com_DataReceived;
        }

        //private readonly static object syncObj = new object();
        //private static void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    lock (syncObj)
        //    {
        //        int count = com.BytesToRead;
        //        byte[] buff = new byte[count];
        //        com.Read(buff, 0, count);
        //        foreach (var item in buff)
        //        {
        //            if (timeOut) break;
        //            buffer.Enqueue(item);
        //        }
        //    }
        //}

        public static SerialPort GetSerialPortInstance()
        {
            if (com == null)
            {
                com = new SerialPort();
                com.DtrEnable = true;
                //com.DataReceived += Com_DataReceived;
            }
            return com;
        }

        public static void Send(SerialCommand<byte[]> command)
        {
            try
            {
                if (!com.IsOpen)
                    com.Open();

                //if (timeOut)
                //    PreHandleTimeOut();
                while (!string.IsNullOrEmpty(com.ReadExisting()))
                {
                    Thread.Sleep(5);
                }

                com.Write(command.Command, 0, command.Command.Length);
                currentCommand = command;
                //timeOut = false;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        //发送之前预处理
        //private static void PreHandleTimeOut()
        //{
        //    com.ReadExisting();
        //    while(buffer.Count > 0)
        //    {
        //        buffer.TryDequeue(out byte _);
        //    }
        //}

        /// <summary>
        /// 异步获取数据。在确定的等待时间内，如果获取到的数据长度不足，抛出TimeoutException异常。
        /// </summary>
        /// <returns></returns>
        public async static Task<byte[]> GetResponseBytes()
        {
            //using (System.Timers.Timer timer = new System.Timers.Timer(waitTime))
            //using (CancellationTokenSource cts = new CancellationTokenSource())
            //{
            //    timer.AutoReset = false;
            //    timer.Elapsed += (s, e) => cts.Cancel();

            //    List<byte> datas = new List<byte>();
            //    await Task.Run(() =>
            //    {
            //        timer.Start();
            //        while (datas.Count < currentCommand.ResponseLength)
            //        {
            //            if (cts.IsCancellationRequested) break;

            //            if (buffer.TryDequeue(out byte data))
            //                datas.Add(data);
            //        }
            //    }, cts.Token);

            //    timer.Stop();

            //    if (datas.Count < currentCommand.ResponseLength)
            //    {
            //        timeOut = true;
            //        throw new TimeoutException();
            //    }

            //    return datas.ToArray();
            //}  
            await Task.Delay(waitTime);
            if(com.BytesToRead < currentCommand.ResponseLength)
            {
                //timeOut = true;
                throw new TimeoutException();
            }
            int count = com.BytesToRead;
            byte[] rets = new byte[count];
            com.Read(rets, 0, count);
            return rets;
        }
    }
}
