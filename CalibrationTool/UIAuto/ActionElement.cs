using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class ActionElement : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get => base["Name"] as string;
            set => base["Name"] = value;
        }

        [ConfigurationProperty("Description")]
        public string Description
        {
            get => base["Description"] as string;
            set => base["Description"] = value;
        }

        [ConfigurationProperty("Format", IsRequired = true)]
        public string Format
        {
            get => base["Format"] as string;
            set => base["Format"] = value;
        }
    }
}
