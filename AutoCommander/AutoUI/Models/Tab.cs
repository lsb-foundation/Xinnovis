using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

public class Tab : IAutoBuild<StackPanel>
{
    [XmlAttribute]
    public string Header { get; set; }

    [XmlArray]
    public List<Group> Groups { get; set; }

    [XmlElement]
    public FileReader FileReader { get; set; }

    public event Action<IAutoBuilder> Executed;

    private StackPanel control;

    public StackPanel Build()
    {
        if (control != null) return control;

        control = new()
        {
            CanVerticallyScroll = true,
            Orientation = Orientation.Vertical
        };
        if (FileReader != null)
        {
            FileReader.Parent = this;
            _ = control.Children.Add(FileReader.Build());
        }
        foreach (Group group in Groups)
        {
            group.Executed += build => this.Executed?.Invoke(build);
            _ = control.Children.Add(group.Build());
        }

        return control;
    }
}
