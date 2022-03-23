using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

public class Group : IAutoBuild<GroupBox>
{
    [XmlAttribute]
    public string Header { get; set; }

    [XmlArray]
    public List<Command> Commands { get; set; }

    public event Action<IAutoBuilder> Executed;

    public GroupBox Build()
    {
        StackPanel stackPanel = new() { Orientation = Orientation.Vertical };
        foreach (Command command in Commands)
        {
            command.Executed += build => this.Executed?.Invoke(build);
            _ = stackPanel.Children.Add(command.Build());
        }

        return new()
        {
            Header = this.Header,
            Content = stackPanel
        };
    }
}
