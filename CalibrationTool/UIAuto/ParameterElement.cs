using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class ParameterElement : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get => base["Name"] as string;
            set => base["Name"] = value;
        }

        [ConfigurationProperty("Description", IsRequired = true)]
        public string Description
        {
            get => base["Description"] as string;
            set => base["Description"] = value;
        }

        [ConfigurationProperty("Type", IsRequired = true)]
        public string Type
        {
            get => base["Type"] as string;
            set => base["Type"] = value;
        }

        [ConfigurationProperty("DefaultValue")]
        public string DefaultValue
        {
            get => base["DefaultValue"] as string;
            set => base["DefaultValue"] = value;
        }

        public string Value { get; set; }
    }
}
