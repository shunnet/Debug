using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Snet.Iot.Debug.behaviors
{
    public static class TabItemRightClickBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(TabItemRightClickBehavior),
                new PropertyMetadata(null, OnCommandChanged));


        public static void SetCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }


        public static ICommand GetCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }


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
