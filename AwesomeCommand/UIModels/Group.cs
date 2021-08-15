using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AwesomeCommand.UIModels
{
    public class Group : IAutoBuild<Expander>
    {
        [XmlAttribute]
        public string Header { get; set; }

        [XmlArray]
        public List<Command> Commands { get; set; }

        public event Action<IAutoBuilder> Executed;

        public Expander Build()
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            foreach (Command command in Commands)
            {
                command.Executed += build => this.Executed?.Invoke(build);
                _ = stackPanel.Children.Add(command.Build());
            }

            Expander expander = new Expander
            {
                Header = this.Header,
                IsExpanded = true,
                Content = stackPanel
            };
            //GroupBox groupBox = new GroupBox
            //{
            //    Header = this.Header,
            //    Content = stackPanel
            //};

            return expander;
        }
    }
}
