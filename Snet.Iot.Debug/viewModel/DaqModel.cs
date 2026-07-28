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
            if ((await daq.GetBaseObjectAsync()).ResultData == null)
            {
                daq = (await daq.CreateInstanceAsync(BasicsData.ToJson(true))).ResultData.GetSource<IDaq>();
            }
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
        public void SetObject(string tag)
        {
            LanguageHandler_OnLanguageEventAsync(null, null);
            InitBasicsData(tag);
            this.tag = tag;
            //加载编码类型
            foreach (var item in typeof(EncodingType).EnumToList())
            {
                ComboBoxItemsSource.Add(new ComboBoxModel(item.Name, item.Value));
            }
            //下拉框默认值
            ComboBoxSelectedItem = ComboBoxItemsSource[0];

            // 图表操作
            chartOperate = ChartOperate.Instance(new()
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
            chartOperate.Style(SkinHandler.GetSkin());

            // 界面消息处理
            uiMessage_DataEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => DataEvent = e.Message;
            uiMessage_DataEvent.StartAsync();
            uiMessage_InfoEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InfoEvent = e.Message;
            uiMessage_InfoEvent.StartAsync();
            uiMessage_InteractionEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InteractionEvent = e.Message;
            uiMessage_InteractionEvent.StartAsync();

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
                    daq = new OpcUaClientOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new OpcUaClientData.Basics();
                    break;
                case "OpcDa":
                    daq = new OpcDaClientOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new OpcDaClientData.Basics();
                    break;
                case "OpcDaHttp":
                    daq = new OpcDaHttpOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new OpcDaHttpData.Basics();
                    break;
                case "TEP":
                    daq = new TepMasterOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new TepMasterData.Basics();
                    break;
                case "Sim":
                    daq = new SimOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new SimData.Basics();
                    break;
                case "DB":
                    daq = new DBOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new DBData.Basics();
                    break;
                case "性能测试":
                    daq = new PerformanceTestingOperate();
                    InteractionVisibility = Visibility.Collapsed;
                    BasicsData = new PerformanceTestingData.Basics();
                    break;
                case "罗克韦尔":
                    daq = new AllenBradleyOperate();
                    BasicsData = new AllenBradleyData.Basics();
                    break;
                case "倍福":
                    daq = new BeckhoffOperate();
                    BasicsData = new BeckhoffData.Basics();
                    break;
                case "西蒙":
                    daq = new CimonOperate();
                    BasicsData = new CimonData.Basics();
                    break;
                case "台达":
                    daq = new DeltaOperate();
                    BasicsData = new DeltaData.Basics();
                    break;
                case "永宏":
                    daq = new FatekOperate();
                    BasicsData = new FatekData.Basics();
                    break;
                case "富士":
                    daq = new FujiOperate();
                    BasicsData = new FujiData.Basics();
                    break;
                case "通用电气":
                    daq = new GEOperate();
                    BasicsData = new GEData.Basics();
                    break;
                case "汇川":
                    daq = new InovanceOperate();
                    BasicsData = new InovanceData.Basics();
                    break;
                case "英威腾":
                    daq = new InvtOperate();
                    BasicsData = new InvtData.Basics();
                    break;
                case "基恩士":
                    daq = new KeyenceOperate();
                    BasicsData = new KeyenceData.Basics();
                    break;
                case "LSis":
                    daq = new LSisOperate();
                    BasicsData = new LSisData.Basics();
                    break;
                case "麦格米特":
                    daq = new MegMeetOperate();
                    BasicsData = new MegMeetData.Basics();
                    break;
                case "三菱":
                    daq = new MitsubishiOperate();
                    BasicsData = new MitsubishiData.Basics();
                    break;
                case "Modbus":
                    daq = new ModbusOperate();
                    BasicsData = new ModbusData.Basics();
                    break;
                case "欧姆龙":
                    daq = new OmronOperate();
                    BasicsData = new OmronData.Basics();
                    break;
                case "松下":
                    daq = new PanasonicOperate();
                    BasicsData = new PanasonicData.Basics();
                    break;
                case "电力通讯规约":
                    daq = new PQDIFOperate();
                    BasicsData = new PQDIFData.Basics();
                    break;
                case "西门子":
                    daq = new SiemensOperate();
                    BasicsData = new SiemensData.Basics();
                    break;
                case "丰田":
                    daq = new ToyotaOperate();
                    BasicsData = new ToyotaData.Basics();
                    break;
                case "丰炜":
                    daq = new VigorOperate();
                    BasicsData = new VigorData.Basics();
                    break;
                case "维控":
                    daq = new WeConOperate();
                    BasicsData = new WeConData.Basics();
                    break;
                case "信捷":
                    daq = new XinJEOperate();
                    BasicsData = new XinJEData.Basics();
                    break;
                case "山武":
                    daq = new YamatakeOperate();
                    BasicsData = new YamatakeData.Basics();
                    break;
                case "安川":
                    daq = new YaskawaOperate();
                    BasicsData = new YaskawaData.Basics();
                    break;
                case "横河":
                    daq = new YokogawaOperate();
                    BasicsData = new YokogawaData.Basics();
                    break;
                case "图尔克":
                    daq = new TurckOperate();
                    BasicsData = new TurckData.Basics();
                    break;
                case "理化":
                    daq = new RKCOperate();
                    BasicsData = new RKCData.Basics();
                    break;
                case "自由协议":
                    daq = new FreedomOperate();
                    BasicsData = new FreedomData.Basics();
                    break;
                case "发那科":
                    daq = new Snet.Fanuc.FanucOperate();
                    BasicsData = new Snet.Fanuc.FanucData.Basics();
                    break;
                case "科伺":
                    daq = new KossiOperate();
                    BasicsData = new KossiData.Basics();
                    break;
                case "东方马达":
                    daq = new OrientalMotorOperate();
                    BasicsData = new OrientalMotorData.Basics();
                    break;
                case "宇电":
                    daq = new YuDianOperate();
                    BasicsData = new YuDianData.Basics();
                    break;
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
