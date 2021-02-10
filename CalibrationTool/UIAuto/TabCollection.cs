using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class TabCollection : ConfigurationElementCollection
    {
        public TabElement this[int index]
        {
            get => BaseGet(index) as TabElement;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(value);
            }
        }

        public new TabElement this[string header]
        {
            get => BaseGet(header) as TabElement;
            set
            {
                if (BaseGet(header) != null)
                {
                    BaseRemove(header);
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TabElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as TabElement).Header;
        }
    }
}
