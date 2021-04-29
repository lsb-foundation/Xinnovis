using System.Collections.Generic;
using System.Linq;

namespace CommonLib.Communication.Serial
{
    public class BaudRateCode
    {
        public int Code { get; set; }
        public int BaudRate { get; set; }

        public BaudRateCode(int code, int baudRate)
        {
            Code = code;
            BaudRate = baudRate;
        }

        private static readonly int[] baudRates = new int[]
        {
            4800,9600,14400,19200,38400,56000,57600,
            115200,128000,230400,256000,460800,500000
        };

        public static List<BaudRateCode> GetBaudRateCodes()
        {
            return baudRates.Select((r, i) => new BaudRateCode(i + 1, r)).ToList();
        }

        public static List<int> GetBaudRates()
        {
            return baudRates.ToList();
        }
    }
}
