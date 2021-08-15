using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AwesomeCommand.UIModels
{
    public class Command : IAutoBuild<Grid>
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlArray]
        public List<Parameter> Parameters { get; set; }

        [XmlArray]
        [XmlArrayItem("Action")]
        public List<UIAction> Actions { get; set; }

        public event Action<IAutoBuilder> Executed;

        public Grid Build()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int index = 0; index < Parameters.Count; index++)
            {
                Parameter parameter = Parameters[index];
                parameter.SetParentCommand(this);
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                DependencyObject[] controls = parameter.Build();
                Label label = controls[0] as Label;
                TextBox textBox = controls[1] as TextBox;
                _ = grid.Children.Add(label);
                _ = grid.Children.Add(textBox);
                Grid.SetRow(label, index);
                Grid.SetRow(textBox, index);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(textBox, 1);
            }

            for (int index = 0; index < Actions.Count; index++)
            {
                UIAction act = Actions[index];
                act.SetParent(this);
                act.Executed += build => this.Executed?.Invoke(build);
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Button button = act.Build();
                _ = grid.Children.Add(button);
                Grid.SetRow(button, Parameters.Count + index);
                Grid.SetColumn(button, 0);
                Grid.SetColumnSpan(button, 2);
            }

            return grid;
        }
    }
}
