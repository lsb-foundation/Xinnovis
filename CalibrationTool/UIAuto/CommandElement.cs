using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CalibrationTool.Models;
using CalibrationTool.ViewModels;
using CommonLib.Mvvm;

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

        public event Action<CommunicationDataType, object> CommandButtonClicked;
        private readonly List<TextBox> _textBoxList = new List<TextBox>();
        private readonly List<Parameter> _parameterDescriptions = new List<Parameter>();

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
                TextBox textBox = new TextBox() { Name = parameter.Name + "TextBox" };
                grid.Children.Add(label);
                grid.Children.Add(textBox);
                Grid.SetRow(label, index);
                Grid.SetRow(textBox, index);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(textBox, 1);
                _textBoxList.Add(textBox);
                _parameterDescriptions.Add(new Parameter
                {
                    Name = parameter.Name,
                    Description = parameter.Description,
                    Type = parameter.Type
                });
            }

            for (int index = 0; index < Actions.Count; index++)
            {
                ActionElement action = Actions[index];
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Button button = new Button
                {
                    Content = action.Description,
                    Tag = action.Format
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
            try
            {
                string format = (sender as Button).Tag as string;
                string convertedFormat = format;
                MatchCollection matches = Regex.Matches(format, @"{\w+}");

                foreach (Match match in matches)
                {
                    string parameter = match.Value.Trim('{', '}');

                    if (!(_textBoxList.FirstOrDefault(box => box.Name == parameter + "TextBox") is TextBox textBox))
                    {
                        throw new Exception($"参数{parameter}未找到。");
                    }

                    var parameterInstance = _parameterDescriptions.FirstOrDefault(p => p.Name == parameter);
                    string input = textBox.Text.Trim();
                    if (string.IsNullOrEmpty(input))
                    {
                        throw new Exception($"参数{parameterInstance.Description}为空。");
                    }

                    bool canParse = false;
                    object parsedValue = null;
                    switch (parameterInstance.Type.Trim().ToLower())
                    {
                        case "int":
                            canParse = int.TryParse(input, out int intNumber);
                            parsedValue = intNumber;
                            break;
                        case "float":
                            canParse = float.TryParse(input, out float floatNumber);
                            parsedValue = floatNumber;
                            break;
                        case "string":
                            parsedValue = input;
                            canParse = true;
                            break;
                        default:
                            throw new Exception($"输入参数[{parameterInstance.Description}]配置的类型{parameterInstance.Type}暂不支持。");
                    }

                    if (!canParse)
                    {
                        throw new Exception($"输入参数[{parameterInstance.Description}]的类型不正确。");
                    }

                    convertedFormat = convertedFormat.Replace(match.Value, match.Value.Replace(parameter, parsedValue.ToString()));
                }
                CommunicationDataType dataType = CommunicationDataType.ASCII;
                if (Type?.Trim().ToUpper() == "HEX")
                {
                    dataType = CommunicationDataType.Hex;
                }
                CommandButtonClicked?.Invoke(dataType, convertedFormat);
            }
            catch(Exception ex)
            {
                ViewModelBase.GetViewModelInstance<StatusBarViewModel>().ShowStatus(ex.Message);
            }
        }

        class Parameter
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
        }
    }
}
