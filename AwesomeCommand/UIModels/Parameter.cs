using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AwesomeCommand.UIModels
{
    public class Parameter : IAutoBuild<DependencyObject[]>
    {
        #region Fields
        [XmlIgnore]
        private DependencyObject[] _control;
        [XmlIgnore]
        private string _value;
        #endregion

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public string Type { get; set; }    //int,float,string,excel

        [XmlAttribute]
        public string DefaultValue { get; set; }

        [XmlAttribute]
        public string DataRange { get; set; }  //读取文件的范围

        [XmlAttribute]
        public string Seperator { get; set; }   //读物文件使用的分割符

        [XmlIgnore]
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                (_control[1] as TextBox).Text = value;
            }
        }

        [XmlIgnore]
        public Command Parent { get; private set; }

        internal void SetParentCommand(Command cmd) => Parent = cmd;

        public DependencyObject[] Build()
        {
            Label label = new Label { Content = Description };
            TextBox textBox = new TextBox { Text = DefaultValue };
            textBox.TextChanged += (s, e) => Value = (s as TextBox).Text;
            _control = new DependencyObject[] { label, textBox };
            Value = DefaultValue;
            return _control;
        }

        public bool TryParse(out object value)
        {
            bool canParse;
            switch (Type.Trim().ToLower())
            {
                case "int":
                    canParse = int.TryParse(Value, out int v);
                    value = v;
                    break;
                case "float":
                    canParse = float.TryParse(Value, out float f);
                    value = f;
                    break;
                case "excel":
                case "string":
                    canParse = true;
                    value = Value;
                    break;
                default:
                    canParse = false;
                    value = null;
                    break;
            }
            return canParse;
        }
    }
}
