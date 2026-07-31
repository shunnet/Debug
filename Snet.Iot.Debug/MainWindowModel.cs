using CommunityToolkit.Mvvm.Input;
using Snet.Iot.Debug.handler;
using Snet.Iot.Debug.view;
using Snet.Iot.Debug.viewModel;
using Snet.Model.data;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.handler;
using Snet.Windows.Core.mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace Snet.Iot.Debug
{
    public class MainWindowModel : BindNotify
    {
        public MainWindowModel()
        {
            // 初始化菜单项数据源
            MenuItemsSource = MenuItemsOperate(App.LanguageOperate);   //给菜单项赋值
            FooterMenuItemsSource = FooterMenuItemsOperate(App.LanguageOperate);  //给底部菜单项赋值
        }
        /// <summary>
        /// 界面加载
        /// </summary>
        public IAsyncRelayCommand NavigationView_Loaded => navigationView_Loaded ??= new AsyncRelayCommand<object>(NavigationView_LoadedAsync);
        private IAsyncRelayCommand? navigationView_Loaded;
        private async Task NavigationView_LoadedAsync(object? sender)
        {
            await Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                NavigationView view = sender.GetSource<RoutedEventArgs>().Source.GetSource<NavigationView>();
                if (App.tabDeviceModel == null)
                {
                    var tabDevice = ControlFinder.FindControlsInLogicalTree<Snet.Iot.Debug.TabDeviceControl>(view).FirstOrDefault();
                    App.tabDeviceModel = tabDevice?.DataContext.GetSource<TabDeviceControlModel>();
                }
                if (App.tabDeviceModel != null)
                {
                    App.tabDeviceModel.AddDevice("关于", InjectionWpf.GetService<About>());
                }
            }));
        }


        /// <summary>
        /// 选中触发
        /// </summary>
        public IAsyncRelayCommand NavigationView_SelectionChanged => navigationView_SelectionChanged ??= new AsyncRelayCommand<object>(NavigationView_SelectionChangedAsync);
        private IAsyncRelayCommand? navigationView_SelectionChanged;
        private async Task NavigationView_SelectionChangedAsync(object? sender)
        {
            NavigationView view = sender.GetSource<RoutedEventArgs>().Source.GetSource<NavigationView>();
            if (App.tabDeviceModel == null)
            {
                var tabDevice = ControlFinder.FindControlsInLogicalTree<Snet.Iot.Debug.TabDeviceControl>(view).FirstOrDefault();
                App.tabDeviceModel = tabDevice?.DataContext.GetSource<TabDeviceControlModel>();
            }
            if (App.tabDeviceModel != null)
            {
                if (view.SelectedItem.TargetPageTag == "关于")
                {
                    App.tabDeviceModel.SetSelected(view.SelectedItem.TargetPageTag, InjectionWpf.GetService<About>());
                }
                else
                {
                    string sTag = view.SelectedItem.TargetPageTag;
                    switch (view.SelectedItem.NavigationViewItemParent.TargetPageTag)
                    {
                        case "Daq":
                            Daq daq = InjectionWpf.GetService<Daq>();
                            DaqModel daqModel = daq.DataContext.GetSource<DaqModel>();
                            await daqModel.SetObjectAsync(sTag);
                            App.tabDeviceModel.AddDevice(sTag, daq);
                            break;
                        case "DaqService":
                            if (sTag == "OpcUaService")
                            {
                                OpcUaService opcUaService = InjectionWpf.GetService<OpcUaService>();
                                App.tabDeviceModel.AddDevice(sTag, opcUaService);
                            }
                            break;
                        case "Mq":
                            Mq mq = InjectionWpf.GetService<Mq>();
                            MqModel mqModel = mq.DataContext.GetSource<MqModel>();
                            await mqModel.SetObjectAsync(sTag);
                            App.tabDeviceModel.AddDevice(sTag, mq);
                            break;
                        case "MqService":
                            if (sTag == "MqttService")
                            {
                                MqttService mqttService = InjectionWpf.GetService<MqttService>();
                                App.tabDeviceModel.AddDevice(sTag, mqttService);
                            }
                            else if (sTag == "MqttWsService")
                            {
                                MqttWebSocketService mqttWebSocketService = InjectionWpf.GetService<MqttWebSocketService>();
                                App.tabDeviceModel.AddDevice(sTag, mqttWebSocketService);
                            }
                            else
                            {
                                NettyService nettyService = InjectionWpf.GetService<NettyService>();
                                App.tabDeviceModel.AddDevice(sTag, nettyService);
                            }
                            break;
                        case "通信":
                            Communication communication = InjectionWpf.GetService<Communication>();
                            CommunicationModel communicationModel = communication.DataContext.GetSource<CommunicationModel>();
                            await communicationModel.SetObjectAsync(sTag);
                            App.tabDeviceModel.AddDevice(sTag, communication);
                            break;
                        case "通信服务端":
                            CommunicationService communicationService = InjectionWpf.GetService<CommunicationService>();
                            CommunicationServiceModel communicationServiceModel = communicationService.DataContext.GetSource<CommunicationServiceModel>();
                            communicationServiceModel.SetObjectAsync(sTag);
                            App.tabDeviceModel.AddDevice(sTag, communicationService);
                            break;
                        case "工具":
                            if (sTag == "Svg")
                            {
                                Svg svg = InjectionWpf.GetService<Svg>();
                                App.tabDeviceModel.AddDevice(sTag, svg);
                            }
                            else if (sTag == "Gif")
                            {
                                Gif gif = InjectionWpf.GetService<Gif>();
                                App.tabDeviceModel.AddDevice(sTag, gif);
                            }
                            else if (sTag == "OpcUaNodeBrowsing")
                            {
                                OpcUaNodeBrowsing opcUaNodeBrowsing = InjectionWpf.GetService<OpcUaNodeBrowsing>();
                                App.tabDeviceModel.AddDevice(sTag, opcUaNodeBrowsing);
                            }
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// 菜单项数据源
        /// </summary>
        public ICollection<object> MenuItemsSource
        {
            get => GetProperty(() => MenuItemsSource);
            set => SetProperty(() => MenuItemsSource, value);
        }
        /// <summary>
        /// 底部菜单项数据源
        /// </summary>
        public ICollection<object> FooterMenuItemsSource
        {
            get => GetProperty(() => FooterMenuItemsSource);
            set => SetProperty(() => FooterMenuItemsSource, value);
        }

        /// <summary>
        /// 菜单项操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ObservableCollection<object> MenuItemsOperate(LanguageModel model)
        {
            // 1. 定义菜单结构
            var menuData = new[]
            {
                new
                {
                    Title = "Daq",
                    ContentStringFormat ="Daq",
                    Icon = SymbolRegular.CatchUp24,
                    SubIcon = SymbolRegular.Molecule28,
                    SubNames = new[] { "OpcUa","OpcDa","OpcDaHttp", "TEP", "Sim", "DB", "性能测试", "Modbus", "西门子","三菱", "欧姆龙", "罗克韦尔", "汇川", "倍福", "电力通讯规约",
                        "台达","西蒙", "永宏", "富士", "通用电气", "英威腾", "基恩士", "LSis", "麦格米特", "松下", "丰田", "丰炜",
                        "维控", "信捷", "山武", "安川", "横河", "图尔克", "理化", "自由协议", "发那科",
                        "科伺", "东方马达", "宇电" }
                },
                new
                {
                    Title = "DaqService",
                    ContentStringFormat ="DaqService",
                    Icon = SymbolRegular.CatchUp24,
                    SubIcon = SymbolRegular.Molecule28,
                    SubNames = new[] { "OpcUaService" }
                },
                new
                {
                    Title = "Mq",
                    ContentStringFormat ="Mq",
                    Icon = SymbolRegular.Flowchart24,
                    SubIcon = SymbolRegular.PlayMultiple16,
                    SubNames = new[] { "Mqtt", "Netty", "RabbitMQ", "Kafka", "NetMQ" }
                },
                new
                {
                    Title = "MqService",
                    ContentStringFormat ="MqService",
                    Icon = SymbolRegular.Flowchart24,
                    SubIcon = SymbolRegular.PlayMultiple16,
                    SubNames = new[] { "MqttService", "MqttWsService", "NettyService" }
                },
                new
                {
                    Title = "通信",
                    ContentStringFormat ="通信",
                    Icon = SymbolRegular.Communication16,
                    SubIcon = SymbolRegular.ArrowTrendingSparkle24,
                    SubNames = new[] { "Tcp", "Ws", "UdpClient", "UdpBroadcast", "UdpMulticast", "Serial" }
                },
                new
                {
                    Title = "通信服务端",
                    ContentStringFormat ="通信服务端",
                    Icon = SymbolRegular.Communication16,
                    SubIcon = SymbolRegular.ArrowTrendingSparkle24,
                    SubNames = new[] { "TcpService", "WsService", "UdpService" }
                },
                new
                {
                    Title = "工具",
                    ContentStringFormat ="工具",
                    Icon = SymbolRegular.Toolbox28,
                    SubIcon = SymbolRegular.SplitVertical20,
                    SubNames = new[] { "OpcUaNodeBrowsing", "Svg", "Gif" }
                }
            };


            var collection = new ObservableCollection<object>();

            foreach (var menu in menuData)
            {
                // 3. 生成子菜单项
                var subItems = menu.SubNames
                    .Select(name => WpfUiHandler.CreationControl(name, menu.SubIcon, App.tabDeviceType, true, model))
                    .Cast<object>()
                    .ToArray();

                // 4. 构建顶级导航项
                var navItem = new NavigationViewItem
                {
                    NavigationCacheMode = NavigationCacheMode.Required,
                    Content = menu.Title,
                    ContentStringFormat = menu.ContentStringFormat,
                    Icon = new SymbolIcon { Symbol = menu.Icon },
                    MenuItemsSource = subItems
                };

                collection.Add(navItem);
            }

            return collection;
        }

        /// <summary>
        /// 底部菜单项操作
        /// </summary>
        /// <returns>返回新的菜单项</returns>
        public ObservableCollection<object> FooterMenuItemsOperate(LanguageModel model) => new ObservableCollection<object> { WpfUiHandler.CreationControl("关于", SymbolRegular.Info28, App.tabDeviceType, true, model) };
    }
}