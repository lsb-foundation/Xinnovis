using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace CalibrationTool.UIAuto
{
    public class GroupElement : ConfigurationElement, IBuildControl
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


        [ConfigurationProperty("Commands")]
        [ConfigurationCollection(typeof(CommandCollection), AddItemName = "Command")]
        public CommandCollection Commands
        {
            get => this["Commands"] as CommandCollection;
        }

        public DependencyObject Build()
        {
            StackPanel groupStackPanel = new StackPanel { Orientation = Orientation.Vertical };
            GroupBox groupBox = new GroupBox
            {
                Header = string.IsNullOrWhiteSpace(Description) ? Name : Description,
                Content = groupStackPanel
            };
            return groupBox;
        }
    }
}
