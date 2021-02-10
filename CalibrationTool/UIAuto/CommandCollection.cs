using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class CommandCollection : ConfigurationElementCollection
    {
        public CommandElement this[int index]
        {
            get => BaseGet(index) as CommandElement;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(value);
            }
        }

        public new CommandElement this[string name]
        {
            get => BaseGet(name) as CommandElement;
            set
            {
                if (BaseGet(name) != null)
                {
                    BaseRemove(name);
                }
                BaseAdd(value);
            }
        }

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
