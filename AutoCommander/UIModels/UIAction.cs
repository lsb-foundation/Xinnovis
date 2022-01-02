using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.UIModels
{
    public class UIAction : IAutoBuild<Button>
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public string Format { get; set; }

        [XmlAttribute]
        public bool IsAuthorized { get; set; }

        [XmlAttribute]
        public bool IsConfirmed { get; set; }

        [XmlAttribute]
        public string Handler { get; set; }

        [XmlIgnore]
        public Command Parent { get; set; }

        public event Action<IAutoBuilder> Executed;

        private string _command;
        public string Command => _command;

        public Button Build()
        {
            Button button = new Button { Content = Description };
            button.Click += (s, e) => Executed?.Invoke(this);
            return button;
        }

        public (bool, string) TryParse()
        {
            string cmd = Format;
            MatchCollection matches = Regex.Matches(Format, @"{\w+}");
            foreach (Match match in matches)
            {
                string pName = match.Value.Trim('{', '}');
                if (Parent.Parameters.FirstOrDefault(p => p.Name == pName) is Parameter parameter)
                {
                    if (string.IsNullOrWhiteSpace(parameter.Value))
                    {
                        return (false, $"参数{parameter.Description}为空");
                    }

                    if (parameter.TryParse(out object value))
                    {
                        cmd = cmd.Replace(match.Value, value.ToString());
                    }
                    else
                    {
                        return (false, $"参数[{parameter.Description}]类型不正确");
                    }
                }
                else
                {
                    return (false, $"参数缺失: {pName}");
                }
            }
            _command = cmd;
            return (true, null);
        }

        public IActionHandler CreateHandler()
        {
            if (string.IsNullOrWhiteSpace(Handler)) return null;

            var handlerType =
                        Assembly.GetEntryAssembly()
                            .GetTypes()
                            .Where(t => t.GetInterface("IActionHandler") != null)
                            .FirstOrDefault(t => t.FullName.ToUpper().EndsWith(Handler.ToUpper()));

            if (handlerType is null)
            {
                MessageBox.Show($"Action {Name}的Handler属性配置错误！未找到{Handler}处理程序。", "配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var handler = Assembly.GetEntryAssembly().CreateInstance(handlerType.FullName) as IActionHandler;
            handler.Command = this.Command;
            return handler;
        }
    }
}
