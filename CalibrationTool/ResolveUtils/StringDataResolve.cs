using System.Text;

namespace CalibrationTool.ResolveUtils
{
    public class StringDataResolve : IResolve<byte[],string>
    {
        public string Resolve(byte[] data)
        {
            return Encoding.Default.GetString(data);
        }
    }
}
