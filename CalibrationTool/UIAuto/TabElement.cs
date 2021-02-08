using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace CalibrationTool.UIAuto
{
    public class TabElement : ConfigurationElement, IBuildControl
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get => base["Name"] as string;
            set => base["Name"] = value;
        }

        [ConfigurationProperty("Description")]
        public string Description
        {
            get => base["Description"] as string;
            set => base["Description"] = value;
        }

        [ConfigurationProperty("Groups", IsRequired = true)]
        [ConfigurationCollection(typeof(GroupCollection), AddItemName = "Group")]
        public GroupCollection Groups
        {
            get => this["Groups"] as GroupCollection;
        }

        public DependencyObject Build()
        {
            TabItem tabItem = new TabItem
            {
                Header = string.IsNullOrWhiteSpace(Description) ? Name : Description,
                Content = new StackPanel()
            };
            return tabItem;
        }
    }
}
