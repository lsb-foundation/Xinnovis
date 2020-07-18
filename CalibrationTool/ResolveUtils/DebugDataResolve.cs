using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationTool.ResolveUtils
{
    public class DebugDataResolve : IResolve<byte[], KeyValuePair<string,string>>
    {
        public KeyValuePair<string,string> Resolve(byte[] data)
        {
            string dataStr = Encoding.Default.GetString(data);
            string[] splitData = dataStr.Trim().Split(':');
            if (splitData.Length != 2)
                return default;

            return new KeyValuePair<string, string>(splitData[0], splitData[1]);
        }
    }
}
