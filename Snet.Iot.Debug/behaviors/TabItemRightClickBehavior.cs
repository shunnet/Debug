using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Snet.Iot.Debug.behaviors
{
    /// <summary>把 TabItem 右键点击映射为可绑定命令。</summary>
    public static class TabItemRightClickBehavior
    {
        /// <summary>附加命令依赖属性。</summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(TabItemRightClickBehavior),
                new PropertyMetadata(null, OnCommandChanged));


        /// <summary>设置右键命令。</summary>
        public static void SetCommand(DependencyObject element, ICommand? value)
        {
            element.SetValue(CommandProperty, value);
        }


        /// <summary>取得右键命令；未设置时返回 <see langword="null"/>。</summary>
        public static ICommand? GetCommand(DependencyObject element)
        {
            return element.GetValue(CommandProperty) as ICommand;
        }


        /// <summary>命令变化时同步事件订阅。</summary>
        private static void OnCommandChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not TabItem tabItem)
                return;


            tabItem.PreviewMouseRightButtonDown -= TabItem_PreviewMouseRightButtonDown;

            if (e.NewValue != null)
            {
                tabItem.PreviewMouseRightButtonDown += TabItem_PreviewMouseRightButtonDown;
            }
        }


        /// <summary>选中右键页签并执行其命令。</summary>
        private static void TabItem_PreviewMouseRightButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not TabItem tabItem)
                return;


            // 先选中
            tabItem.IsSelected = true;


            var command = GetCommand(tabItem);

            if (command?.CanExecute(tabItem.DataContext) == true)
            {
                command.Execute(tabItem.DataContext);
            }
        }
    }
}
