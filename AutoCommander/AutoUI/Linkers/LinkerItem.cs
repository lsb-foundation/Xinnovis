using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Linkers;

public class LinkerItem
{
    [XmlAttribute]
    public string Command { get; set; }

    [XmlAttribute]
    public string Handler { get; set; }

    [XmlAttribute]
    public string Description { get; set; }

    public string Display()
    {
        return string.IsNullOrWhiteSpace(Description) ?
            Handler : Description;
    }

    public IActionHandler CreateHandler()
    {
        if (string.IsNullOrWhiteSpace(Handler)) return null;

        var handlerType =
                    Assembly.GetEntryAssembly()
                        .GetTypes()
                        .Where(t => t.GetInterface("IActionHandler") != null)
                        .FirstOrDefault(t => t.FullName.ToUpper().EndsWith(Handler.ToUpper()));

        if (handlerType is null)
        {
            throw new Exception($"[配置错误]LinkerItem[Command={Command}]的Handler属性配置错误！未找到{Handler}处理程序。");
        }

        var handler = Assembly.GetEntryAssembly().CreateInstance(handlerType.FullName) as IActionHandler;
        handler.Command = this.Command;
        return handler;
    }
}
