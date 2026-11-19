using Microsoft.Extensions.DependencyInjection;
using Snet.Iot.Debug.model;
using Snet.Iot.Debug.view;
using Snet.Windows.Core.handler;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Snet.Iot.Debug.Tests;

/// <summary>导航重复加载时默认页签初始化的回归测试。</summary>
[TestClass]
public sealed class NavigationLoadedTests
{
    /// <summary>重复 Loaded 不干扰页签，实际窗口双向换肤后仍能导航。</summary>
    [STATestMethod]
    public void Loaded_InitializesAboutOnceAndPreservesSelection()
    {
        var application = new Application();
        var previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(application.Dispatcher));
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

            VerifyNavigationAfterThemeChange(application);
        }
        finally
        {
            foreach (var tab in tabs.Devices) tab.Dispose();
            App.tabDeviceModel = previousModel;
            application.Shutdown();
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    /// <summary>使用实际窗口验证换肤后的采集、工具、关于页和返回导航。</summary>
    private static void VerifyNavigationAfterThemeChange(Application application)
    {
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Snet.Windows.Core;component/themes/DarkTheme.xaml")
        });
        IconsHandler.Loading(App.IconResourcePath);
        InjectionWpf.AddService(services =>
        {
            services.AddTransient<Daq>();
            services.AddTransient<Svg>();
        });
        App.tabDeviceModel = null;

        var model = new MainWindowModel();
        var window = new MainWindow { DataContext = model, ShowInTaskbar = false };
        var initialSkin = SkinHandler.GetSkin();
        try
        {
            window.Show();
            DrainDispatcher();
            var navigation = (NavigationView)window.FindName("NavigationViewControls");
            var group = (NavigationViewItem)navigation.MenuItems[0]!;
            if (!group.IsExpanded) Click(group);
            DrainDispatcher();
            var opcUa = (NavigationViewItem)group.MenuItems[0]!;
            var opcDa = (NavigationViewItem)group.MenuItems[1]!;
            if (ReferenceEquals(navigation.SelectedItem, opcUa))
            {
                Click(opcDa);
                WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            }
            Click(opcUa);
            WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            DrainDispatcher();
            var opcUaTab = AssertShowing(navigation, "OpcUa");
            var initialBackground = ((System.Windows.Media.SolidColorBrush)window.Background).Color;

            WaitFor(window.SkinCommand.ExecuteAsync(null));
            DrainDispatcher();
            Assert.AreNotEqual(initialBackground, ((System.Windows.Media.SolidColorBrush)window.Background).Color);
            Assert.AreSame(opcUaTab, AssertShowing(navigation, "OpcUa"));
            Assert.IsTrue(navigation.CanGoBack, "换肤应保留已有导航历史。");
            Click(opcDa);
            WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            DrainDispatcher();
            Assert.AreSame(opcDa, navigation.SelectedItem);
            var opcDaTab = AssertShowing(navigation, "OpcDa");

            WaitFor(window.SkinCommand.ExecuteAsync(null));
            DrainDispatcher();
            Assert.AreEqual(initialSkin, SkinHandler.GetSkin());
            Assert.AreSame(opcDaTab, AssertShowing(navigation, "OpcDa"));
            Click(opcUa);
            WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            DrainDispatcher();
            AssertShowing(navigation, "OpcUa");
            Assert.AreEqual(1, App.tabDeviceModel!.Devices.Count(tab => tab.NameKey == "关于"));

            Click((ButtonBase)navigation.Template.FindName("PART_BackButton", navigation));
            WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            DrainDispatcher();
            AssertShowing(navigation, "OpcDa");

            var paneToggle = (ButtonBase)navigation.Template.FindName("PART_ToggleButton", navigation);
            var paneWasOpen = navigation.IsPaneOpen;
            Click(paneToggle);
            DrainDispatcher();
            Assert.AreNotEqual(paneWasOpen, navigation.IsPaneOpen);
            Click(paneToggle);
            DrainDispatcher();
            Assert.AreEqual(paneWasOpen, navigation.IsPaneOpen);

            var toolsGroup = navigation.MenuItems.OfType<NavigationViewItem>()
                .Single(item => item.ContentStringFormat == "工具");
            if (!toolsGroup.IsExpanded) Click(toolsGroup);
            DrainDispatcher();
            Click(toolsGroup.MenuItems.OfType<NavigationViewItem>().Single(item => item.TargetPageTag == "Svg"));
            WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            DrainDispatcher();
            AssertShowing(navigation, "Svg");

            Click((NavigationViewItem)navigation.FooterMenuItems[0]!);
            WaitFor(model.NavigationView_SelectionChanged.ExecutionTask!);
            DrainDispatcher();
            AssertShowing(navigation, "关于");
            Assert.AreEqual(1, App.tabDeviceModel!.Devices.Count(tab => tab.NameKey == "关于"));
        }
        finally
        {
            if (App.tabDeviceModel is { } windowTabs)
                WaitFor(windowTabs.CloseAllTabCommand.ExecuteAsync(null));
            window.Close();
            SkinHandler.SetSkin(initialSkin);
        }
    }

    /// <summary>检查右侧显示的真实 WPF 页签与模型选中项一致。</summary>
    private static TabControlDeviceModel AssertShowing(NavigationView navigation, string expectedTag)
    {
        var presenter = (NavigationViewContentPresenter)navigation.Template.FindName("PART_NavigationViewContentPresenter", navigation);
        var tabHost = (TabDeviceControl)presenter.Content;
        var tabControl = (TabControl)tabHost.FindName("DeviceTabs");
        var tab = (TabControlDeviceModel)tabControl.SelectedContent;
        Assert.AreEqual(expectedTag, tab.NameKey);
        Assert.AreSame(App.tabDeviceModel?.SelectedDevicesItem, tab);
        Assert.IsTrue(tab.Content.IsVisible);
        return tab;
    }

    /// <summary>调用导航按钮实际用于鼠标和键盘输入的 Click 入口。</summary>
    private static void Click(ButtonBase button) =>
        typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, null);

    /// <summary>等待已排队的模板和导航事件完成。</summary>
    private static void DrainDispatcher() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

    /// <summary>处理 Dispatcher 队列，等待 Loaded 命令的异步初始化完成。</summary>
    private static void ExecuteLoaded(MainWindowModel model, RoutedEventArgs args) =>
        WaitFor(model.NavigationView_Loaded.ExecuteAsync(args));

    /// <summary>在 STA 线程等待命令时继续处理 WPF 消息。</summary>
    private static void WaitFor(Task task)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        _ = task.WaitAsync(TimeSpan.FromSeconds(15)).ContinueWith(_ =>
            dispatcher.BeginInvoke(new Action(() => frame.Continue = false)),
            TaskScheduler.Default);
        Dispatcher.PushFrame(frame);
        Assert.IsTrue(task.IsCompleted, "等待 WPF 命令超时。");
        task.GetAwaiter().GetResult();
    }
}
