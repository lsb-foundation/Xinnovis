using System.Configuration;

namespace MFCSoftware.Common
{
    public class AppSettings
    {
        public static string AppName { get; set; }
        public static bool AllowLowerFlowValue { get; set; }    //是否允许设定小于满量程1%的流量值
        public static bool ReadTemperature { get; set; }

        static AppSettings()
        {
            foreach (var property in typeof(AppSettings).GetProperties())
            {
                string setValue = ConfigurationManager.AppSettings[property.Name];
                if (property.PropertyType == typeof(bool))
                {
                    bool value = !string.IsNullOrEmpty(setValue) && setValue.Equals("true");
                    property.SetValue(new AppSettings(), value);
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(new AppSettings(), setValue);
                }
                else if (property.PropertyType == typeof(int) && int.TryParse(setValue, out int value))
                {
                    property.SetValue(new AppSettings(), value);
                }
            }
        }
    }
}
