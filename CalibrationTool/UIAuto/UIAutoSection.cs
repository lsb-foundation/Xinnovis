using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class UIAutoSection : ConfigurationSection
    {
        [ConfigurationProperty("Tabs")]
        [ConfigurationCollection(typeof(TabCollection), AddItemName = "Tab")]
        public TabCollection Tabs
        {
            get => base["Tabs"] as TabCollection;
        }
    }
}
