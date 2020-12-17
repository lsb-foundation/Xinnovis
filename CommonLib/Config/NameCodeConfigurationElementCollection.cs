using System.Configuration;

namespace CommonLib.Config
{
    public class NameCodeConfigurationElementCollection : ConfigurationElementCollection
    {
        public NameCodeConfigurationElement this[int index]
        {
            get => BaseGet(index) as NameCodeConfigurationElement;
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(value);
            }
        }

        public new NameCodeConfigurationElement this[string name]
        {
            get => BaseGet(name) as NameCodeConfigurationElement;
            set
            {
                if (BaseGet(name) != null)
                    BaseRemoveAt(BaseIndexOf(BaseGet(name)));
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NameCodeConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as NameCodeConfigurationElement).Name;
        }
    }
}
