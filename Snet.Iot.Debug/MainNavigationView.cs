using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Snet.Iot.Debug
{
    /// <summary>在主窗口换肤时保留导航索引、返回历史及事件订阅。</summary>
    public sealed class MainNavigationView : NavigationView
    {
        /// <summary>等模板和加载事件处理完，再确认导航控件是否真正离开窗口。</summary>
        protected override void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // WPF-UI 4.3 在 Unloaded 中永久解绑事件并清空导航索引，
            // 但换肤重建窗口模板时，同一个导航控件会重新加载。
            _ = Dispatcher.InvokeAsync(() =>
            {
                if (!IsLoaded)
                {
                    base.OnUnloaded(sender, e);
                }
            }, DispatcherPriority.Background);
        }
    }
}
