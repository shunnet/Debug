using Microsoft.Extensions.DependencyInjection;
using Snet.Iot.Debug.view;
using Snet.Windows.Core.handler;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Snet.Iot.Debug.Tests;

/// <summary>导航重复加载时默认页签初始化的回归测试。</summary>
[TestClass]
public sealed class NavigationLoadedTests
{
    /// <summary>重复 Loaded 不新增关于页、不抢占设备页，关闭后也不自动重开。</summary>
    [STATestMethod]
    public void Loaded_InitializesAboutOnceAndPreservesSelection()
    {
        var application = new Application();
        var previousModel = App.tabDeviceModel;
        var tabs = new TabDeviceControlModel();
        try
        {
            InjectionWpf.AddService(services => services.AddSingleton<About>());
            var model = new MainWindowModel();
            var view = new NavigationView();
            var args = new RoutedEventArgs(FrameworkElement.LoadedEvent, view);

            // 首次未找到容器不能消耗初始化机会。
            App.tabDeviceModel = null;
            ExecuteLoaded(model, args);
            App.tabDeviceModel = tabs;
            ExecuteLoaded(model, args);
            Assert.HasCount(1, tabs.Devices);
            Assert.AreEqual("关于", tabs.SelectedDevicesItem.NameKey);

            tabs.AddDevice("测试设备", new UserControl());
            var selectedDevice = tabs.SelectedDevicesItem;
            ExecuteLoaded(model, args);
            ExecuteLoaded(model, args);
            Assert.HasCount(2, tabs.Devices);
            Assert.AreSame(selectedDevice, tabs.SelectedDevicesItem);

            var aboutTab = tabs.Devices.Single(tab => tab.NameKey == "关于");
            tabs.Devices.Remove(aboutTab);
            aboutTab.Dispose();
            ExecuteLoaded(model, args);
            Assert.HasCount(1, tabs.Devices);
            Assert.AreSame(selectedDevice, tabs.SelectedDevicesItem);

            // 导航选中事件先创建关于页时，首次 Loaded 也不能重复添加或抢占选中页。
            tabs.AddDevice("关于", InjectionWpf.GetService<About>());
            tabs.SelectedDevicesItem = selectedDevice;
            ExecuteLoaded(new MainWindowModel(), args);
            Assert.HasCount(2, tabs.Devices);
            Assert.AreSame(selectedDevice, tabs.SelectedDevicesItem);
        }
        finally
        {
            foreach (var tab in tabs.Devices) tab.Dispose();
            App.tabDeviceModel = previousModel;
            application.Shutdown();
        }
    }

    /// <summary>处理 Dispatcher 队列，等待 Loaded 命令的异步初始化完成。</summary>
    private static void ExecuteLoaded(MainWindowModel model, RoutedEventArgs args)
    {
        Task task = model.NavigationView_Loaded.ExecuteAsync(args);
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        _ = task.ContinueWith(_ => dispatcher.BeginInvoke(new Action(() => frame.Continue = false)),
            TaskScheduler.Default);
        Dispatcher.PushFrame(frame);
        task.GetAwaiter().GetResult();
    }
}
