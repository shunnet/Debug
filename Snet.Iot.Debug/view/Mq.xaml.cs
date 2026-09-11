using Snet.Utility;
using Snet.Windows.Controls.edit;
using Snet.Windows.Controls.handler;
using System.Windows.Controls;
using System.Windows.Input;

namespace Snet.Iot.Debug.view
{
    /// <summary>
    /// Mq.xaml 的交互逻辑
    /// </summary>
    public partial class Mq : UserControl
    {
        private readonly List<EditHandler> editHandlers = [];

        /// <summary>初始化消息队列客户端视图并接管编辑器扩展的生命周期。</summary>
        public Mq()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>控件进入可视树时安装编辑器扩展。</summary>
        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (editHandlers.Count != 0) return;
            editHandlers.Add(new EditHandler(edit1, App.EditModels, color: ("#454545", "#FEFEFE")));
            editHandlers.Add(new EditHandler(edit2, App.EditModels, color: ("#454545", "#FEFEFE")));
        }

        /// <summary>控件离开可视树时解除编辑器与静态主题事件。</summary>
        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (EditHandler handler in editHandlers) handler.Dispose();
            editHandlers.Clear();
        }

        /// <summary>
        /// 拦截文本输入，防止用户手动编辑日志内容
        /// </summary>
        private void TextEditor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 拦截键盘按键，阻止粘贴（Ctrl+V）、删除和退格操作
        /// </summary>
        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) || e.Key == Key.Delete || e.Key == Key.Back)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 文本内容变化时自动滚动到末尾，保持最新日志可见
        /// </summary>
        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TextEditor text = sender.GetSource<TextEditor>();
            text.SelectionStart = text.Text.Length;
            text.SelectionLength = 0;
            text.ScrollToEnd();
        }
    }
}
