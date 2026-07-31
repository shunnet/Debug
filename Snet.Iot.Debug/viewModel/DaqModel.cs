using CommunityToolkit.Mvvm.Input;
using ScottPlot.WPF;
using Snet.AllenBradley;
using Snet.Beckhoff;
using Snet.Cimon;
using Snet.Core.handler;
using Snet.DB;
using Snet.Delta;
using Snet.Driver.Core.Net;
using Snet.Fatek;
using Snet.Freedom;
using Snet.Fuji;
using Snet.GE;
using Snet.Inovance;
using Snet.Invt;
using Snet.Iot.Debug.chart;
using Snet.Keyence;
using Snet.Kossi;
using Snet.LSis;
using Snet.MegMeet;
using Snet.Mitsubishi;
using Snet.Modbus;
using Snet.Model.data;
using Snet.Model.@enum;
using Snet.Model.@interface;
using Snet.Omron;
using Snet.Opc.da.client;
using Snet.Opc.da.http;
using Snet.Opc.ua.client;
using Snet.OrientalMotor;
using Snet.Panasonic;
using Snet.PerformanceTesting;
using Snet.PQDIF;
using Snet.RKC;
using Snet.Siemens;
using Snet.Sim;
using Snet.TEP.master;
using Snet.Toyota;
using Snet.Turck;
using Snet.Utility;
using Snet.Vigor;
using Snet.WeCon;
using Snet.Windows.Controls.data;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.handler;
using Snet.Windows.Core.mvvm;
using Snet.XinJE;
using Snet.Yamatake;
using Snet.Yaskawa;
using Snet.Yokogawa;
using Snet.YuDian;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using YSAI.PerformanceTesting;

namespace Snet.Iot.Debug.viewModel
{
    public class DaqModel : BindNotify, IDisposable, IAsyncDisposable
    {
        #region 属性
        /// <summary>
        /// 间隔
        /// </summary>
        private int _interval = 500;

        /// <summary>
        /// 图表操作
        /// </summary>
        private ChartOperate chartOperate;

        /// <summary>
        /// 采集对象
        /// 外部设置好传进来
        /// </summary>
        private IDaq daq;

        /// <summary>
        /// 标识符
        /// </summary>
        private string tag;

        /// <summary>
        /// ui信息处理器
        /// </summary>
        private UiMessageHandler uiMessage_InfoEvent = new UiMessageHandler("InfoEvent");
        private UiMessageHandler uiMessage_DataEvent = new UiMessageHandler("DataEvent");
        private UiMessageHandler uiMessage_InteractionEvent = new UiMessageHandler("InteractionEvent");

        /// <summary>
        /// 选中的下标
        /// </summary>
        public int TabSelectedIndex
        {
            get => GetProperty(() => TabSelectedIndex);
            set => SetProperty(() => TabSelectedIndex, value);
        }

        /// <summary>
        /// 是否闪烁
        /// </summary>
        public bool DeviceStatusFlashing
        {
            get => GetProperty(() => DeviceStatusFlashing);
            set => SetProperty(() => DeviceStatusFlashing, value);
        }

        /// <summary>
        /// 设备状态常亮 绿色代表正常
        /// </summary>
        public bool DeviceStatusChangLiang
        {
            get => GetProperty(() => DeviceStatusChangLiang);
            set => SetProperty(() => DeviceStatusChangLiang, value);
        }

        /// <summary>
        /// LED 颜色
        /// </summary>
        public System.Windows.Media.Color LedColor
        {
            get => ledColor;
            set => SetProperty(ref ledColor, value);
        }
        private System.Windows.Media.Color ledColor = System.Windows.Media.Colors.Green;

        /// <summary>
        /// 数据类型
        /// </summary>
        public int DataType
        {
            get => dataType;
            set => SetProperty(ref dataType, value);
        }
        private int dataType = 21;

        /// <summary>
        /// 控件
        /// </summary>
        public WpfPlot ChartControl
        {
            get => chartControl;
            set => SetProperty(ref chartControl, value);
        }
        private WpfPlot chartControl = new WpfPlot();

        /// <summary>
        /// 工具标题
        /// </summary>
        public string ToolTitle
        {
            get => GetProperty(() => ToolTitle);
            set => SetProperty(() => ToolTitle, value);
        }

        /// <summary>
        /// 基础数据
        /// </summary>
        public object BasicsData
        {
            get => GetProperty(() => BasicsData);
            set => SetProperty(() => BasicsData, value);
        }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get => GetProperty(() => Address);
            set => SetProperty(() => Address, value);
        }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length
        {
            get => length;
            set => SetProperty(ref length, value);
        }
        private int length = 1;

        /// <summary>
        /// 数据
        /// </summary>
        public string Data
        {
            get => GetProperty(() => Data);
            set => SetProperty(() => Data, value);
        }

        /// <summary>
        /// 编码类型集合
        /// </summary>
        public ObservableCollection<ComboBoxModel> ComboBoxItemsSource
        {
            get => _ComboBoxItemsSource;
            set => SetProperty(ref _ComboBoxItemsSource, value);
        }
        private ObservableCollection<ComboBoxModel> _ComboBoxItemsSource = new ObservableCollection<ComboBoxModel>();

        /// <summary>
        /// 编码类型
        /// </summary>
        public ComboBoxModel ComboBoxSelectedItem
        {
            get => GetProperty(() => ComboBoxSelectedItem);
            set => SetProperty(() => ComboBoxSelectedItem, value);
        }

        /// <summary>
        /// 交互数据显示状态
        /// </summary>
        public Visibility InteractionVisibility
        {
            get => _interactionVisibility;
            set => SetProperty(ref _interactionVisibility, value);
        }
        private Visibility _interactionVisibility = Visibility.Visible;



        /// <summary>
        /// 信息事件
        /// </summary>
        public string InfoEvent
        {
            get => GetProperty(() => InfoEvent);
            set => SetProperty(() => InfoEvent, value);
        }

        /// <summary>
        /// 数据事件
        /// </summary>
        public string DataEvent
        {
            get => GetProperty(() => DataEvent);
            set => SetProperty(() => DataEvent, value);
        }

        /// <summary>
        /// 交互事件
        /// </summary>
        public string InteractionEvent
        {
            get => GetProperty(() => InteractionEvent);
            set => SetProperty(() => InteractionEvent, value);
        }

        #endregion

        #region 命令
        /// <summary>
        /// 信息清空
        /// </summary>
        public IAsyncRelayCommand InfoClear => p_InfoClear ??= new AsyncRelayCommand(InfoClearAsync);
        IAsyncRelayCommand p_InfoClear;
        public async Task InfoClearAsync()
        {
            await uiMessage_InfoEvent.ClearAsync();
        }

        /// <summary>
        /// 数据清空
        /// </summary>
        public IAsyncRelayCommand DataClear => p_DataClear ??= new AsyncRelayCommand(DataClearAsync);
        IAsyncRelayCommand p_DataClear;
        public async Task DataClearAsync()
        {
            await uiMessage_DataEvent.ClearAsync();
        }

        /// <summary>
        /// 交互清空
        /// </summary>
        public IAsyncRelayCommand InteractionClear => p_InteractionClear ??= new AsyncRelayCommand(InteractionClearAsync);
        IAsyncRelayCommand p_InteractionClear;
        public async Task InteractionClearAsync()
        {
            await uiMessage_InteractionEvent.ClearAsync();
        }

        /// <summary>
        /// 打开
        /// </summary>
        public IAsyncRelayCommand On => p_On ??= new AsyncRelayCommand(OnAsync);
        IAsyncRelayCommand p_On;
        public async Task OnAsync()
        {
            var result = await daq.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
            {
                daq.OnInfoEventAsync -= Daq_OnInfoEventAsync;
                daq.OnInfoEventAsync += Daq_OnInfoEventAsync;
                daq.OnDataEventAsync -= Daq_OnDataEventAsync;
                daq.OnDataEventAsync += Daq_OnDataEventAsync;

                await RegisterEventAsync();
            }

            DeviceStatusFlashing = (await daq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public IAsyncRelayCommand Off => p_Off ??= new AsyncRelayCommand(OffAsync);
        IAsyncRelayCommand p_Off;
        public async Task OffAsync()
        {
            var result = await daq.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
            {
                daq.OnInfoEventAsync -= Daq_OnInfoEventAsync;
                daq.OnDataEventAsync -= Daq_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await daq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 读取
        /// </summary>
        public IAsyncRelayCommand Read => p_Read ??= new AsyncRelayCommand(ReadAsync);
        IAsyncRelayCommand p_Read;
        public async Task ReadAsync()
        {
            Address address = OrganizationAddress();
            OperateResult result = await daq.ReadAsync(address);
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
                await uiMessage_InfoEvent.ShowAsync(result.ResultData.ToJson(true));

            DeviceStatusFlashing = (await daq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 写入
        /// </summary>
        public IAsyncRelayCommand Write => p_Write ??= new AsyncRelayCommand(WriteAsync);
        IAsyncRelayCommand p_Write;
        public async Task WriteAsync()
        {
            ConcurrentDictionary<string, WriteModel> pairs = new ConcurrentDictionary<string, WriteModel>();
            EncodingType encoding = (EncodingType)Enum.Parse(typeof(EncodingType), ComboBoxSelectedItem.Key);
            Model.@enum.DataType dataType = (Model.@enum.DataType)DataType;
            pairs.TryAdd(Address, new WriteModel(Data, dataType, encoding));
            await uiMessage_InfoEvent.ShowAsync((await daq.WriteAsync(pairs)).Message);

            DeviceStatusFlashing = (await daq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 订阅
        /// </summary>
        public IAsyncRelayCommand Subscribe => p_Subscribe ??= new AsyncRelayCommand(SubscribeAsync);
        IAsyncRelayCommand p_Subscribe;
        public async Task SubscribeAsync()
        {
            OperateResult result = await daq.SubscribeAsync(OrganizationAddress());
            await uiMessage_InfoEvent.ShowAsync(result.Message);

            DeviceStatusFlashing = (await daq.GetStatusAsync()).Status;
            TabSelectedIndex = 2;
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public IAsyncRelayCommand UnSubscribe => p_UnSubscribe ??= new AsyncRelayCommand(UnSubscribeAsync);
        IAsyncRelayCommand p_UnSubscribe;
        public async Task UnSubscribeAsync()
        {
            OperateResult result = await daq.UnSubscribeAsync(OrganizationAddress());
            await uiMessage_InfoEvent.ShowAsync(result.Message);

            DeviceStatusFlashing = (await daq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }
        #endregion

        #region 事件

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <returns></returns>
        private async Task RegisterEventAsync()
        {
            if (InteractionVisibility == Visibility.Visible)
            {
                if ((await daq.GetStatusAsync()).Status)
                {
                    BinaryCommunication? baseLog = (await daq.GetBaseObjectAsync()).GetSource<BinaryCommunication>();
                    if (baseLog is not null)
                    {
                        baseLog.LogNet.BeforeSaveToFile -= LogNet_BeforeSaveToFile;
                        baseLog.LogNet.BeforeSaveToFile += LogNet_BeforeSaveToFile;
                    }
                }
            }
        }
        /// <summary>
        /// 底层日志保存前事件
        /// </summary>
        private void LogNet_BeforeSaveToFile(object? sender, Driver.LogNet.SnetEventArgs e)
        {
            string msg = e.SnetMessage.Text;
            //System.DateTime dateTime = e.SnetMessage.Time;

            if (msg.IsNullOrWhiteSpace()) return;

            string log = $"{msg}\r\n";

            uiMessage_InteractionEvent.ShowAsync(log);
        }

        /// <summary>
        /// 数据事件
        /// </summary>
        private async Task Daq_OnDataEventAsync(object? sender, EventDataResult e)
        {
            if (e.GetDetails(out string? message, out ConcurrentDictionary<string, AddressValue>? data))
            {
                foreach (var item in data)
                {
                    await LogAddressValueAsync(item.Value, e.Message);
                }
            }
            else if (e.GetDetails(out message, out List<ConcurrentDictionary<string, AddressValue>>? datas))
            {
                foreach (var items in datas)
                {
                    foreach (var item in items)
                    {
                        await LogAddressValueAsync(item.Value, e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 数据展示
        /// </summary>
        private async Task LogAddressValueAsync(AddressValue value, string message)
        {
            string valueStr = value.AddressDataType switch
            {
                Model.@enum.DataType.ByteArray => ByteHandler.ByteToHexString(value.ResultValue.GetSource<byte[]>(), ' '),
                _ when value.AddressDataType.ToString().Contains("Array") => value.ResultValue.ToJson(),
                _ => value.ResultValue?.ToString() ?? string.Empty
            };


            //添加图表数据
            switch (value.AddressDataType)
            {
                case Model.@enum.DataType.Byte:
                case Model.@enum.DataType.Double:
                case Model.@enum.DataType.Float:
                case Model.@enum.DataType.Single:
                case Model.@enum.DataType.Short:
                case Model.@enum.DataType.Int16:
                case Model.@enum.DataType.Ushort:
                case Model.@enum.DataType.UInt16:
                case Model.@enum.DataType.Int:
                case Model.@enum.DataType.Int32:
                case Model.@enum.DataType.Uint:
                case Model.@enum.DataType.UInt32:
                case Model.@enum.DataType.Long:
                case Model.@enum.DataType.Int64:
                case Model.@enum.DataType.Ulong:
                case Model.@enum.DataType.UInt64:
                    if (value.Quality == QualityType.Normal)
                    {
                        if (!chartOperate.DoesItExist(value.AddressName).Status)
                        {
                            chartOperate.Create(new() { SN = value.AddressName, Title = value.AddressName, TitleEN = value.AddressName });
                        }
                        chartOperate.Update(value.AddressName, Convert.ToDouble(value.OriginalValue));
                    }
                    break;
            }


            string logMessage = $"[ 订阅通知 ]点位数据更新\r\n键：{value.AddressName}\r\n值：{valueStr}\r\n状态：{value.Quality}\r\n消息：{value.Message}\r\n时间：{value.Time.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n";

            await uiMessage_DataEvent.ShowAsync(logMessage);
        }

        /// <summary>
        /// 信息事件
        /// </summary>
        private async Task Daq_OnInfoEventAsync(object? sender, EventInfoResult e)
        {
            if (e.Message == Core.handler.LanguageHandler.GetLanguageValue("日期已变更"))
            {
                await RegisterEventAsync();
            }
            else
            {
                await uiMessage_InfoEvent.ShowAsync(e.ToJson(true));
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 组织地址
        /// </summary>
        /// <returns></returns>
        private Address OrganizationAddress()
        {
            EncodingType encoding = (EncodingType)Enum.Parse(typeof(EncodingType), ComboBoxSelectedItem.Key);
            Model.@enum.DataType dataType = (Model.@enum.DataType)DataType;
            Address address = new Address();
            address.AddressArray = new List<AddressDetails>()
            {
                new AddressDetails()
                {
                    SN=ToolTitle,
                    AddressName = Address,
                    AddressDataType=dataType,
                    Length=Length.ToUshort(),
                    EncodingType=encoding,
                }
            };
            return address;
        }

        /// <summary>
        /// 设置对象
        /// </summary>
        public async Task SetObjectAsync(string tag)
        {
            InitBasicsData(tag);
            this.tag = tag;

            await LanguageHandler_OnLanguageEventAsync(null, null);

            //加载编码类型
            foreach (var item in typeof(EncodingType).EnumToList())
            {
                ComboBoxItemsSource.Add(new ComboBoxModel(item.Name, item.Value));
            }
            //下拉框默认值
            ComboBoxSelectedItem = ComboBoxItemsSource[0];

            // 图表操作
            chartOperate = await ChartOperate.InstanceAsync(new()
            {
                ChartControl = ChartControl,
                LineAdjust = true,
                LineRemove = true,
                DataRemove = true,
                YCrosshairText = true,
                XCrosshairText = true,
                RefreshTime = _interval
            });
            chartOperate.On();
            chartOperate.SetTheme(SkinHandler.GetSkin());

            // 界面消息处理
            uiMessage_DataEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => DataEvent = e.Message;
            await uiMessage_DataEvent.StartAsync();
            uiMessage_InfoEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InfoEvent = e.Message;
            await uiMessage_InfoEvent.StartAsync();
            uiMessage_InteractionEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InteractionEvent = e.Message;
            await uiMessage_InteractionEvent.StartAsync();

            Core.handler.LanguageHandler.OnLanguageEventAsync -= LanguageHandler_OnLanguageEventAsync;
            Core.handler.LanguageHandler.OnLanguageEventAsync += LanguageHandler_OnLanguageEventAsync;
        }

        private async Task LanguageHandler_OnLanguageEventAsync(object? sender, EventLanguageResult e)
        {
            string title = (await Core.handler.LanguageHandler.GetLanguageAsync()) == LanguageType.zh ? " 调试工具" : " Debug Tool";
            ToolTitle = (await App.LanguageOperate.GetLanguageValueAsync(tag)) + title;
        }

        /// <summary>
        /// 初始化实例参数
        /// </summary>
        /// <param name="tag">名称</param>
        /// <returns></returns>
        private void InitBasicsData(string tag)
        {
            switch (tag)
            {
                case "OpcUa":
                    {
                        InteractionVisibility = Visibility.Collapsed;
                        var obj = new OpcUaClientData.Basics();
                        daq = new OpcUaClientOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "OpcDa":
                    {
                        var obj = new OpcDaClientData.Basics();
                        InteractionVisibility = Visibility.Collapsed;
                        daq = new OpcDaClientOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "OpcDaHttp":
                    {
                        InteractionVisibility = Visibility.Collapsed;
                        var obj = new OpcDaHttpData.Basics();
                        daq = new OpcDaHttpOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "TEP":
                    {
                        InteractionVisibility = Visibility.Collapsed;
                        var obj = new TepMasterData.Basics();
                        daq = new TepMasterOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "Sim":
                    {
                        InteractionVisibility = Visibility.Collapsed;
                        var obj = new SimData.Basics();
                        daq = new SimOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "DB":
                    {
                        InteractionVisibility = Visibility.Collapsed;
                        var obj = new DBData.Basics();
                        daq = new DBOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "性能测试":
                    {
                        InteractionVisibility = Visibility.Collapsed;
                        var obj = new PerformanceTestingData.Basics();
                        daq = new PerformanceTestingOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "罗克韦尔":
                    {
                        var obj = new AllenBradleyData.Basics();
                        daq = new AllenBradleyOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "倍福":
                    {
                        var obj = new BeckhoffData.Basics();
                        daq = new BeckhoffOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "西蒙":
                    {
                        var obj = new CimonData.Basics();
                        daq = new CimonOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "台达":
                    {
                        var obj = new DeltaData.Basics();
                        daq = new DeltaOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "永宏":
                    {
                        var obj = new FatekData.Basics();
                        daq = new FatekOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "富士":
                    {
                        var obj = new FujiData.Basics();
                        daq = new FujiOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "通用电气":
                    {
                        var obj = new GEData.Basics();
                        daq = new GEOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "汇川":
                    {
                        var obj = new InovanceData.Basics();
                        daq = new InovanceOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "英威腾":
                    {
                        var obj = new InvtData.Basics();
                        daq = new InvtOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "基恩士":
                    {
                        var obj = new KeyenceData.Basics();
                        daq = new KeyenceOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "LSis":
                    {
                        var obj = new LSisData.Basics();
                        daq = new LSisOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "麦格米特":
                    {
                        var obj = new MegMeetData.Basics();
                        daq = new MegMeetOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "三菱":
                    {
                        var obj = new MitsubishiData.Basics();
                        daq = new MitsubishiOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "Modbus":
                    {
                        var obj = new ModbusData.Basics();
                        daq = new ModbusOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "欧姆龙":
                    {
                        var obj = new OmronData.Basics();
                        daq = new OmronOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "松下":
                    {
                        var obj = new PanasonicData.Basics();
                        daq = new PanasonicOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "电力通讯规约":
                    {
                        var obj = new PQDIFData.Basics();
                        daq = new PQDIFOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "西门子":
                    {
                        var obj = new SiemensData.Basics();
                        daq = new SiemensOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "丰田":
                    {
                        var obj = new ToyotaData.Basics();
                        daq = new ToyotaOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "丰炜":
                    {
                        var obj = new VigorData.Basics();
                        daq = new VigorOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "维控":
                    {
                        var obj = new WeConData.Basics();
                        daq = new WeConOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "信捷":
                    {
                        var obj = new XinJEData.Basics();
                        daq = new XinJEOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "山武":
                    {
                        var obj = new YamatakeData.Basics();
                        daq = new YamatakeOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "安川":
                    {
                        var obj = new YaskawaData.Basics();
                        daq = new YaskawaOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "横河":
                    {
                        var obj = new YokogawaData.Basics();
                        daq = new YokogawaOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "图尔克":
                    {
                        var obj = new TurckData.Basics();
                        daq = new TurckOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "理化":
                    {
                        var obj = new RKCData.Basics();
                        daq = new RKCOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "自由协议":
                    {
                        var obj = new FreedomData.Basics();
                        daq = new FreedomOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "发那科":
                    {
                        var obj = new Snet.Fanuc.FanucData.Basics();
                        daq = new Snet.Fanuc.FanucOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "科伺":
                    {
                        var obj = new KossiData.Basics();
                        daq = new KossiOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "东方马达":
                    {
                        var obj = new OrientalMotorData.Basics();
                        daq = new OrientalMotorOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "宇电":
                    {
                        var obj = new YuDianData.Basics();
                        daq = new YuDianOperate(obj);
                        BasicsData = obj;
                        break;
                    }
            }
        }


        public void Dispose()
        {
            try
            {
                daq?.Dispose();
            }
            catch { }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (daq != null)
                {
                    await daq.DisposeAsync();
                }
            }
            catch { }
        }
        #endregion
    }
}
