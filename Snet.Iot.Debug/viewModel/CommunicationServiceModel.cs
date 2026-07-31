using CommunityToolkit.Mvvm.Input;
using Snet.Core.communication.net.core;
using Snet.Core.communication.net.@enum;
using Snet.Core.communication.net.tcp.service;
using Snet.Core.communication.net.udp.unicast.service;
using Snet.Core.communication.net.ws.service;
using Snet.Core.handler;
using Snet.Model.data;
using Snet.Model.@enum;
using Snet.Model.@interface;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Snet.Iot.Debug.viewModel
{
    public class CommunicationServiceModel : BindNotify, IDisposable, IAsyncDisposable
    {

        #region 属性
        /// <summary>
        /// 对象
        /// 外部设置好传进来
        /// </summary>
        private ICommunicationService communication;

        /// <summary>
        /// 标识符
        /// </summary>
        private string tag;

        /// <summary>
        /// ui信息处理器
        /// </summary>
        private UiMessageHandler uiMessage_InfoEvent = new UiMessageHandler("InfoEvent");
        private UiMessageHandler uiMessage_DataEvent = new UiMessageHandler("DataEvent");


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
        /// 发送数据格式
        /// 0 ASCII
        /// 1 HEX
        /// </summary>
        public int DataFormat
        {
            get => GetProperty(() => DataFormat);
            set => SetProperty(() => DataFormat, value);
        }

        /// <summary>
        /// 发送的数据
        /// </summary>
        public string Data
        {
            get => GetProperty(() => Data);
            set => SetProperty(() => Data, value);
        }

        /// <summary>
        /// 群发
        /// </summary>
        public bool MassSend
        {
            get => GetProperty(() => MassSend);
            set => SetProperty(() => MassSend, value);
        }

        /// <summary>
        /// 表格数据
        /// </summary>
        public ObservableCollection<DataGridStructuralBody> DataGridItemsSource
        {
            get => dataGridItemsSource;
            set => SetProperty(ref dataGridItemsSource, value);
        }
        private ObservableCollection<DataGridStructuralBody> dataGridItemsSource = new ObservableCollection<DataGridStructuralBody>();
        /// <summary>
        /// 选中的行数据
        /// </summary>
        public DataGridStructuralBody DataGridSelectionItem = new DataGridStructuralBody();

        /// <summary>
        /// DataGrid结构体
        /// </summary>
        public class DataGridStructuralBody : BindNotify
        {
            public string IpAddress
            {
                get => GetProperty(() => IpAddress);
                set => SetProperty(() => IpAddress, value);
            }

            public int Port
            {
                get => GetProperty(() => Port);
                set => SetProperty(() => Port, value);
            }

            /// <summary>
            /// 地址与端口
            /// </summary>
            public string IPENDPORT { get; set; }
        }
        #endregion

        #region 命令

        /// <summary>
        /// 表格数据选中触发
        /// </summary>
        public IAsyncRelayCommand GridDataSelectionChanged => p_GridDataSelectionChanged ??= new AsyncRelayCommand<SelectionChangedEventArgs>(GridDataSelectionChangedAsync);
        IAsyncRelayCommand p_GridDataSelectionChanged;
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="e"></param>
        private Task GridDataSelectionChangedAsync(SelectionChangedEventArgs? e)
        {
            DataGridSelectionItem = e.Source.GetSource<DataGrid>().SelectedItem.GetSource<DataGridStructuralBody>();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 打开
        /// </summary>
        public IAsyncRelayCommand On => p_On ??= new AsyncRelayCommand(OnAsync);
        IAsyncRelayCommand p_On;
        public async Task OnAsync()
        {
            var result = await communication.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
            {
                communication.OnInfoEventAsync -= Communication_OnInfoEventAsync;
                communication.OnInfoEventAsync += Communication_OnInfoEventAsync;
                communication.OnDataEventAsync -= Communication_OnDataEventAsync;
                communication.OnDataEventAsync += Communication_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await communication.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public IAsyncRelayCommand Off => p_Off ??= new AsyncRelayCommand(OffAsync);
        IAsyncRelayCommand p_Off;
        public async Task OffAsync()
        {
            var result = await communication.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
            {
                communication.OnInfoEventAsync -= Communication_OnInfoEventAsync;
                communication.OnDataEventAsync -= Communication_OnDataEventAsync;
                DataGridItemsSource.Clear();
            }

            DeviceStatusFlashing = (await communication.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 发送
        /// </summary>
        public IAsyncRelayCommand Send => p_Send ??= new AsyncRelayCommand(SendAsync);
        IAsyncRelayCommand p_Send;
        public async Task SendAsync()
        {
            if (Data.IsNullOrWhiteSpace())
            {
                await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("数据不能为空"));
                return;
            }
            //发送的数据
            byte[] sendData = null;
            if (DataFormat.Equals(0))
            {
                sendData = Encoding.ASCII.GetBytes(Data);
            }
            else
            {
                if (Data.IsHexadecimal())
                {
                    sendData = Data.ToHex(false);
                }
                else
                {
                    await uiMessage_InfoEvent.ShowAsync($"“{Data}”{App.LanguageOperate.GetLanguageValue("不是有效的 Hex 数据")}");
                }
            }

            if (MassSend)
            {
                OperateResult result = await communication.SendAsync(sendData);
                await uiMessage_InfoEvent.ShowAsync(result.ToJson(true));
            }
            else
            {
                if (DataGridSelectionItem != null && !string.IsNullOrEmpty(DataGridSelectionItem.IPENDPORT))
                {
                    OperateResult operateResult = await communication.SendAsync(sendData, DataGridSelectionItem.IPENDPORT);
                    await uiMessage_InfoEvent.ShowAsync(operateResult.ToJson(true));
                }
                else
                {
                    await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("请选择一个已连接的客户端"));
                }
            }

            DeviceStatusFlashing = (await communication.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

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
        #endregion

        #region 事件
        private async Task Communication_OnDataEventAsync(object? sender, EventDataResult e)
        {
            await uiMessage_DataEvent.ShowAsync(e.Message.Replace("[", "[ ").Replace("]", " ] "));

            ClientMessage? message = e.GetSource<ClientMessage>();
            if (message == null)
            {
                return;
            }
            string[] ipport;
            switch (message.Step)
            {
                case Steps.客户端连接:
                    ipport = message.IpPort.Split(':');
                    if (Application.Current == null)
                        return;
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DataGridItemsSource.Add(new DataGridStructuralBody() { IpAddress = ipport[0], Port = ipport[1].ToInt(), IPENDPORT = message.IpPort });
                    });
                    break;
                case Steps.客户端断开:
                    if (Application.Current == null)
                        return;
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        for (int i = 0; i < DataGridItemsSource.Count; i++)
                        {
                            if (DataGridItemsSource[i].IPENDPORT.Equals(message.IpPort))
                            {
                                DataGridItemsSource.RemoveAt(i);
                                continue;
                            }
                        }
                    });
                    break;
                case Steps.消息接收:
                    if (message.Bytes != null)
                    {
                        if (DataFormat == 0)
                        {
                            await uiMessage_DataEvent.ShowAsync($"[ {message.IpPort} ] -> {Encoding.ASCII.GetString(message.Bytes)}");
                        }
                        else
                        {
                            await uiMessage_DataEvent.ShowAsync($"[ {message.IpPort} ] -> {message.Bytes.ToHexString()}");
                        }
                    }
                    break;
            }

        }

        private async Task Communication_OnInfoEventAsync(object? sender, EventInfoResult e)
        {
            await uiMessage_InfoEvent.ShowAsync(e.ToJson(true));
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置对象
        /// </summary>
        public async Task SetObjectAsync(string tag)
        {
            InitBasicsData(tag);
            this.tag = tag;

            await LanguageHandler_OnLanguageEventAsync(null, null);

            // 界面消息处理
            uiMessage_DataEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => DataEvent = e.Message;
            await uiMessage_DataEvent.StartAsync();
            uiMessage_InfoEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InfoEvent = e.Message;
            await uiMessage_InfoEvent.StartAsync();

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
                case "TcpService":
                    {
                        var obj = new TcpServiceData.Basics();
                        communication = new TcpServiceOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "WsService":
                    {
                        var obj = new WsServiceData.Basics();
                        communication = new WsServiceOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "UdpService":
                    {
                        var obj = new UdpServiceData.Basics();
                        communication = new UdpServiceOperate(obj);
                        BasicsData = obj;
                        break;
                    }
            }
        }
        public void Dispose()
        {
            try
            {
                communication?.Dispose();
            }
            catch { }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (communication != null)
                {
                    await communication.DisposeAsync();
                }
            }
            catch { }
        }
        #endregion
    }
}
