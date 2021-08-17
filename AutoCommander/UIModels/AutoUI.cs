using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.UIModels
{
    [XmlRoot]
    public class AutoUI : IAutoBuild<TabControl>
    {
        [XmlArray]
        public List<Tab> Tabs { get; set; }

        public event Action<IAutoBuilder> Executed;

        public TabControl Build()
        {
            TabControl tabControl = new TabControl();
            foreach (Tab tab in Tabs)
            {
                tab.Executed += build => this.Executed?.Invoke(build);
                _ = tabControl.Items.Add(tab.Build());
            }
            return tabControl;
        }

        public static AutoUI GetUIAuto(string file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AutoUI));
                return serializer.Deserialize(stream) as AutoUI;
            }
        }
    }
}
