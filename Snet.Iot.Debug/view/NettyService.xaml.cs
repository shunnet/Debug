using Snet.Iot.Debug.handler;
using Snet.Windows.Controls.edit;
using Snet.Windows.Controls.handler;
using System.Windows;
using System.Windows.Controls;

namespace Snet.Iot.Debug.view
{
    /// <summary>
    /// NettyServiceView.xaml 的交互逻辑
    /// </summary>
    public partial class NettyService : UserControl
    {
        public NettyService()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var presenter = ControlFinder.FindVisualChild<ContentPresenter>(template);
            if (presenter != null && template.ContentTemplate != null)
            {
                new EditHandler((template.ContentTemplate.FindName("edit1", presenter) as TextEditor), App.EditModels, color: ("#454545", "#FEFEFE"));
                new EditHandler((template.ContentTemplate.FindName("edit2", presenter) as TextEditor), App.EditModels, color: ("#454545", "#FEFEFE"));
            }
        }
    }
}
