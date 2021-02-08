using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class CommandCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CommandElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as CommandElement).Name;
        }
    }
}
