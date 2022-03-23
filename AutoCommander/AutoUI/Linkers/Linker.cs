using AutoCommander.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Linkers;

[XmlRoot]
public class Linker
{
    [XmlArray]
    public List<LinkerItem> Items { get; set; }

    public static Linker CreateLinker()
    {
        string file = PathUtils.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LinkerFile);
        if (!File.Exists(file)) return null;
        using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        var serializer = new XmlSerializer(typeof(Linker));
        return serializer.Deserialize(stream) as Linker;
    }
}
