using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

public class Option
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Value { get; set; }
}
