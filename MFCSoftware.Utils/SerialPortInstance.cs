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

        /// <summary>
        /// 超时等待时间
        /// </summary>
        public static int WaitTime = 100;

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

        private static Task SendAsync(SerialCommand<byte[]> command)
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

        private async static Task<byte[]> GetResponseBytesAsync()
        {
            await Task.Yield();
            Thread.Sleep(WaitTime);
            if (comInstance.BytesToRead < currentCommand.ResponseLength)
            {
                throw new TimeoutException();
            }
            int count = comInstance.BytesToRead;
            byte[] rets = new byte[count];
            _ = comInstance.Read(rets, 0, count);
            return rets;
        }

        /// <summary>
        /// 异步获取数据。在确定的等待时间内，如果获取到的数据长度不足，抛出TimeoutException异常。
        /// </summary>
        /// <returns></returns>
        public static async Task<byte[]> GetResponseAsync(SerialCommand<byte[]> command)
        {
            try
            {
                await ComSharingService.Semaphore.WaitAsync();
                await SendAsync(command);
                var bytes = await GetResponseBytesAsync();
                return bytes;
            }
            finally
            {
                ComSharingService.Semaphore.Release();
            }
        }
    }
}
