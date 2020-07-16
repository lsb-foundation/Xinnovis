using System.Text;

namespace CalibrationTool.Models
{
    public class ASCIIStringDataParse : IParse<string,string>
    {
        public string Resolve(string data)
        {
            return Encoding.Default.GetString(Encoding.ASCII.GetBytes(data));
        }
    }
}
