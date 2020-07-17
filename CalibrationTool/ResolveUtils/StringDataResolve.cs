using System.Text;

namespace CalibrationTool.ResolveUtils
{
    public class StringDataResolve : IResolve<string,string>
    {
        public string Resolve(string data)
        {
            return Encoding.Default.GetString(Encoding.ASCII.GetBytes(data));
        }
    }
}
