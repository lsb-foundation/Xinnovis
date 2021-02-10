using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class GroupCollection : ConfigurationElementCollection
    {
        public GroupElement this[int index]
        {
            get => BaseGet(index) as GroupElement;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(value);
            }
        }

        public new GroupElement this[string header]
        {
            get => BaseGet(header) as GroupElement;
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
            return new GroupElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as GroupElement).Header;
        }
    }
}
