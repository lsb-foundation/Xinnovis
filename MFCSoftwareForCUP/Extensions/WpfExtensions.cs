using System.Windows;
using System.Windows.Media;

namespace MFCSoftwareForCUP.Extensions
{
    public static class WpfExtensions
    {
        public static DependencyObject GetParentWindow(this DependencyObject dependency)
        {
            return dependency is Window ? dependency : VisualTreeHelper.GetParent(dependency).GetParentWindow();
        }
    }
}
