using AutoCommander.Properties;

namespace AutoCommander.Common;

public class SettingsUtils
{
    public static void Save<T>(string name, T value)
    {
        if (Settings.Default[name] is T)
        {
            Settings.Default[name] = value;
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
