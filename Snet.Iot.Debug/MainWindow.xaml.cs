using Snet.Windows.Controls.handler;
using Snet.Windows.Core;

namespace Snet.Iot.Debug
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigationViewControls.SelectNavigationViewDefaultItem(this, App.tabDeviceType, App.LanguageOperate, "mainGrid");
        }
    }
}