using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Snet.Iot.Debug.handler
{
    public static class ControlFinder
    {
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        public static IEnumerable<T> FindControlsInLogicalTree<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent is T t) yield return t;

            // 1. 遍历逻辑子元素（包括 ColumnDefinition 等）
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject depChild)
                {
                    foreach (var sub in FindControlsInLogicalTree<T>(depChild))
                        yield return sub;
                }
            }

            // 2. 仅当对象是 Visual 或 Visual3D 时才遍历视觉树
            if (parent is Visual || parent is Visual3D)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    foreach (var sub in FindControlsInLogicalTree<T>(child))
                        yield return sub;
                }
            }
        }
    }
}
