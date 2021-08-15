using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AwesomeCommand.UIModels
{
    public class Tab : IAutoBuild<TabItem>
    {
        [XmlAttribute]
        public string Header { get; set; }

        [XmlArray]
        public List<Group> Groups { get; set; }

        [XmlElement]
        public FileReader FileReader { get; set; }

        public event Action<IAutoBuilder> Executed;

        public TabItem Build()
        {
            StackPanel stackPanel = new StackPanel
            {
                CanVerticallyScroll = true,
                Orientation = Orientation.Vertical
            };
            if (FileReader != null)
            {
                FileReader.SetParent(this);
                _ = stackPanel.Children.Add(FileReader.Build());
            }
            foreach (Group group in Groups)
            {
                group.Executed += build => this.Executed?.Invoke(build);
                _ = stackPanel.Children.Add(group.Build());
            }

            TabItem tabItem = new TabItem
            {
                Header = this.Header,
                Content = stackPanel
            };

            return tabItem;
        }
    }
}
