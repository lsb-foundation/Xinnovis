using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace MFCSoftware.Utils
{
    public static class SerialPortInstance
    {
        private static SerialPort comInstance;
        private static SerialCommand<byte[]> currentCommand = null;
        private const int waitTime = 100;

        static SerialPortInstance()
        {
            comInstance = new SerialPort
            {
                DtrEnable = true
            };
        }

        public static SerialPort GetSerialPortInstance()
        {
            if (comInstance == null)
            {
                comInstance = new SerialPort
                {
                    DtrEnable = true
                };
            }
            return comInstance;
        }

        public static Task SendAsync(SerialCommand<byte[]> command)
        {
            if (!comInstance.IsOpen)
            {
                comInstance.Open();
            }

            while (!string.IsNullOrEmpty(comInstance.ReadExisting()))
            {
                Thread.Sleep(5);
            }

            comInstance.Write(command.Command, 0, command.Command.Length);
            currentCommand = command;

            return Task.CompletedTask;
        }

        /// <summary>
        /// 异步获取数据。在确定的等待时间内，如果获取到的数据长度不足，抛出TimeoutException异常。
        /// </summary>
        /// <returns></returns>
        public async static Task<byte[]> GetResponseBytesAsync()
        {
            await Task.Delay(waitTime);
            if (comInstance.BytesToRead < currentCommand.ResponseLength)
            {
                throw new TimeoutException();
            }
            int count = comInstance.BytesToRead;
            byte[] rets = new byte[count];
            _ = comInstance.Read(rets, 0, count);
            return rets;
        }
    }
}
