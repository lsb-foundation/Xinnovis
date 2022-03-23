using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

public class UIAction : IAutoBuild<Button>
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Description { get; set; }

    [XmlAttribute]
    public string Format { get; set; }

    [XmlAttribute]
    public bool IsConfirmed { get; set; }

    [XmlIgnore]
    public Command Parent { get; set; }

    public event Action<IAutoBuilder> Executed;

    private string _command;
    public string Command => _command;

    public Button Build()
    {
        Button button = new() { Content = Description };
        button.Click += (s, e) => Executed?.Invoke(this);
        return button;
    }

    public void CreateCommand()
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
                    throw new Exception($"参数{parameter.Description}为空");
                }

                if (parameter.TryParse(out object value))
                {
                    cmd = cmd.Replace(match.Value, value.ToString());
                }
                else
                {
                    throw new Exception($"参数[{parameter.Description}]类型不正确");
                }
            }
            else
            {
                throw new Exception($"参数缺失: {pName}");
            }
        }
        _command = cmd;
    }
}
