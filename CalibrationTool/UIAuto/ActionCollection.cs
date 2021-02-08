using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class ActionCollection : ConfigurationElementCollection
    {
        public ActionElement this[int index]
        {
            get => BaseGet(index) as ActionElement;
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(value);
            }
        }

        public new ActionElement this[string name]
        {
            get => BaseGet(name) as ActionElement;
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
            return new ActionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ActionElement).Name;
        }
    }
}
