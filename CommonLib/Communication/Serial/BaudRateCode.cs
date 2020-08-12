using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static int[] baudRates = new int[]
        {
            4800,9600,14400,19200,38400,56000,57600,
            115200,128000,230400,256000,460800,500000
        };

        public static List<BaudRateCode> GetBaudRateCodes()
        {
            List<BaudRateCode> baudRateCodes = new List<BaudRateCode>();
            for (int index = 0; index < baudRates.Length; index++)
            {
                BaudRateCode code = new BaudRateCode(index + 1, baudRates[index]);
                baudRateCodes.Add(code);
            }
            return baudRateCodes;
        }

        public static List<int> GetBaudRates()
        {
            return baudRates.ToList();
        }
    }
}
