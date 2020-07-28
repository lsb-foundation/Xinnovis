using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationTool.ResolveUtils
{
    public class DebugDataResolve : IResolve<byte[], List<KeyValuePair<string,string>>>
    {
        public List<KeyValuePair<string,string>> Resolve(byte[] data)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string dataStr = Encoding.Default.GetString(data);
            string[] lines = dataStr.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
            {
                string[] splitData = line.Trim().Split(':');
                if (splitData.Length == 2)
                {
                    KeyValuePair<string, string> keyValue = new KeyValuePair<string, string>(splitData[0], splitData[1]);
                    result.Add(keyValue);
                }
            }

            return result;
        }
    }
}
