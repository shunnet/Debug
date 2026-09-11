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
        private readonly List<EditHandler> editHandlers = [];

        public NettyService()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (editHandlers.Count != 0)
                return;
            var presenter = ControlFinder.FindVisualChild<ContentPresenter>(template);
            if (presenter != null && template.ContentTemplate != null)
            {
                if (template.ContentTemplate.FindName("edit1", presenter) is TextEditor edit1)
                    editHandlers.Add(new EditHandler(edit1, App.EditModels, color: ("#454545", "#FEFEFE")));
                if (template.ContentTemplate.FindName("edit2", presenter) is TextEditor edit2)
                    editHandlers.Add(new EditHandler(edit2, App.EditModels, color: ("#454545", "#FEFEFE")));
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (EditHandler handler in editHandlers)
                handler.Dispose();
            editHandlers.Clear();
        }
    }
}
