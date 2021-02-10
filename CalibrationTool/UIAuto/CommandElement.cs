using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CalibrationTool.UIAuto
{
    public class CommandElement : ConfigurationElement, IBuildControl
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get => base["Name"] as string;
            set => base["Name"] = value;
        }

        [ConfigurationProperty("Description", IsRequired = true)]
        public string Description
        {
            get => base["Description"] as string;
            set => base["Description"] = value;
        }

        [ConfigurationProperty("Type")]
        public string Type
        {
            get => base["Type"] as string;
            set => base["Type"] = value;
        }

        [ConfigurationProperty("Parameters")]
        [ConfigurationCollection(typeof(ParameterCollection), AddItemName = "Parameter")]
        public ParameterCollection Parameters
        {
            get => this["Parameters"] as ParameterCollection;
        }

        [ConfigurationProperty("Actions")]
        [ConfigurationCollection(typeof(ActionCollection), AddItemName = "Action")]
        public ActionCollection Actions
        {
            get => this["Actions"] as ActionCollection;
        }

        internal event Action<CommandElement, ActionElement, List<ParameterElement>> CommandButtonClicked;
        private readonly List<TextBox> _textBoxList = new List<TextBox>();
        private readonly List<ParameterElement> _parameters = new List<ParameterElement>();

        public DependencyObject Build()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int index = 0; index < Parameters.Count; index++)
            {
                ParameterElement parameter = Parameters[index];
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Label label = new Label { Content = parameter.Description };
                TextBox textBox = new TextBox 
                { 
                    Name = parameter.Name + "TextBox",
                    Text = parameter.DefaultValue
                };
                grid.Children.Add(label);
                grid.Children.Add(textBox);
                Grid.SetRow(label, index);
                Grid.SetRow(textBox, index);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(textBox, 1);
                _textBoxList.Add(textBox);
                _parameters.Add(parameter);
            }

            for (int index = 0; index < Actions.Count; index++)
            {
                ActionElement action = Actions[index];
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Button button = new Button
                {
                    Content = action.Description,
                    Tag = action
                };
                button.Click += Button_Click;
                grid.Children.Add(button);
                Grid.SetRow(button, Parameters.Count + index);
                Grid.SetColumn(button, 0);
                Grid.SetColumnSpan(button, 2);
            }

            return grid;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is ActionElement action)
            {
                foreach (ParameterElement parameter in _parameters)
                {
                    if (_textBoxList.FirstOrDefault(box=>box.Name == parameter.Name + "TextBox") is TextBox textBox)
                    {
                        parameter.Value = textBox.Text;
                    }
                }
                CommandButtonClicked?.Invoke(this, action, _parameters);
            }
        }
    }
}
