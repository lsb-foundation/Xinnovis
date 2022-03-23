using System.IO;

namespace AutoCommander.Common
{
    public class PathUtils
    {
        public static string Combine(params string[] paths)
        {
            string path = Path.Combine(paths);
            string folder = path;
            if (Path.HasExtension(path))
            {
                folder = Directory.GetParent(path).FullName;
            }
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return path;
        }
    }
}
