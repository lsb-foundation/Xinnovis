using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace CalibrationTool.UIAuto
{
    public class UIAutoSection : ConfigurationSection, IBuildControl
    {
        [ConfigurationProperty("Tabs")]
        [ConfigurationCollection(typeof(TabCollection), AddItemName = "Tab")]
        public TabCollection Tabs
        {
            get => base["Tabs"] as TabCollection;
        }

        public event Action<CommandEventArgs> CommandActionInvoked;

        public DependencyObject Build()
        {
            TabControl tabControl = new TabControl();
            foreach (TabElement tabElement in Tabs)
            {
                TabItem tab = tabElement.Build() as TabItem;
                foreach (GroupElement groupElement in tabElement.Groups)
                {
                    GroupBox group = groupElement.Build() as GroupBox;
                    foreach (CommandElement commandElement in groupElement.Commands)
                    {
                        commandElement.CommandButtonClicked += (c, a, ps) => CommandActionInvoked?.Invoke(new CommandEventArgs(c, a, ps));
                        Grid grid = commandElement.Build() as Grid;
                        (group.Content as StackPanel).Children.Add(grid);
                    }
                    (tab.Content as StackPanel).Children.Add(group);
                }
                tabControl.Items.Add(tab);
            }
            return tabControl;
        }
    }
}
