using System.Configuration;

namespace CommonLib.Config
{
    public class NameCodeConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("Collection")]
        [ConfigurationCollection(typeof(NameCodeConfigurationElementCollection), AddItemName = "add")]
        public NameCodeConfigurationElementCollection Collection
        {
            get => this["Collection"] as NameCodeConfigurationElementCollection;
        }

        public static NameCodeConfigurationSection GetConfig(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName) as NameCodeConfigurationSection;
        }
    }
}
