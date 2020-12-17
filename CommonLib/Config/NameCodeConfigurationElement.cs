using System.Configuration;

namespace CommonLib.Config
{
    public class NameCodeConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get => base["name"] as string;
            set => base["name"] = value;
        }

        [ConfigurationProperty("code", IsRequired = true)]
        public int Code
        {
            get => (int)base["code"];
            set => base["code"] = value;
        }
    }
}
