using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib.MfcUtils
{
    public static class SerialPortInstance
    {
        private static SerialPort com;
        private static SerialCommand<byte[]> currentCommand = null;
        private const int waitTime = 100;

        static SerialPortInstance()
        {
            com = new SerialPort
            {
                DtrEnable = true
            };
        }

        public static SerialPort GetSerialPortInstance()
        {
            if (com == null)
            {
                com = new SerialPort
                {
                    DtrEnable = true
                };
            }
            return com;
        }

        public static void Send(SerialCommand<byte[]> command)
        {
            if (!com.IsOpen)
                com.Open();

            while (!string.IsNullOrEmpty(com.ReadExisting()))
            {
                Thread.Sleep(5);
            }

            com.Write(command.Command, 0, command.Command.Length);
            currentCommand = command;
        }

        /// <summary>
        /// 异步获取数据。在确定的等待时间内，如果获取到的数据长度不足，抛出TimeoutException异常。
        /// </summary>
        /// <returns></returns>
        public async static Task<byte[]> GetResponseBytes()
        {
            await Task.Delay(waitTime);
            if(com.BytesToRead < currentCommand.ResponseLength)
            {
                throw new TimeoutException();
            }
            int count = com.BytesToRead;
            byte[] rets = new byte[count];
            com.Read(rets, 0, count);
            return rets;
        }
    }
}
