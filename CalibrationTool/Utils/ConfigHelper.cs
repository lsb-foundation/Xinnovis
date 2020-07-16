using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Extensions;

namespace CalibrationTool.Utils
{
    public class ConfigHelper
    {
        private static Configuration config;

        static ConfigHelper()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
        private static void InitializeConfigFile()
        {
            
        }
    }
}
