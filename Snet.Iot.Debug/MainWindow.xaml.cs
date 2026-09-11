using Snet.Windows.Controls.handler;
using Snet.Windows.Core;

namespace Snet.Iot.Debug
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        /// <summary>初始化主窗口并选择默认导航项。</summary>
        public MainWindow()
        {
            InitializeComponent();
            NavigationViewControls.SelectNavigationViewDefaultItem(this, App.tabDeviceType, App.LanguageOperate, "mainGrid");
        }
    }
}
