using System.Configuration;

namespace CalibrationTool.UIAuto
{
    public class ParameterCollection : ConfigurationElementCollection
    {
        public ParameterElement this[int index]
        {
            get => BaseGet(index) as ParameterElement;
            set
            {
                if(BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(value);
            }
        }

        public new ParameterElement this[string name]
        {
            get => BaseGet(name) as ParameterElement;
            set
            {
                if(BaseGet(name) != null)
                {
                    BaseRemove(name);
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ParameterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ParameterElement).Name;
        }
    }
}
