using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

[XmlRoot]
public class AutoUI
{
    [XmlArray]
    public List<Tab> Tabs { get; set; }

    public static AutoUI GetUIAuto(string file)
    {
        using FileStream stream = new(file, FileMode.Open, FileAccess.Read);
        XmlSerializer serializer = new(typeof(AutoUI));
        return serializer.Deserialize(stream) as AutoUI;
    }
}
