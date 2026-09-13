using CommunityToolkit.Mvvm.Input;
using Snet.Core.communication.net.tcp.client;
using Snet.Core.communication.net.udp.broadcast;
using Snet.Core.communication.net.udp.multicast;
using Snet.Core.communication.net.udp.unicast.client;
using Snet.Core.communication.net.ws.client;
using Snet.Core.communication.serial;
using Snet.Core.handler;
using Snet.Model.data;
using Snet.Model.@enum;
using Snet.Model.@interface;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;
using System.Text;

namespace Snet.Iot.Debug.viewModel
{
    public class CommunicationModel : BindNotify, IDisposable, IAsyncDisposable
    {
        #region 属性
        /// <summary>
        /// 对象
        /// 外部设置好传进来
        /// </summary>
        private ICommunication? communication;

        /// <summary>
        /// 标识符
        /// </summary>
        private string tag = string.Empty;
        private int disposed;

        /// <summary>
        /// ui信息处理器
        /// </summary>
        private UiMessageHandler uiMessage_InfoEvent = new UiMessageHandler($"InfoEvent·{Guid.NewGuid().ToString()}");
        private UiMessageHandler uiMessage_DataEvent = new UiMessageHandler($"DataEvent·{Guid.NewGuid().ToString()}");


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
        #endregion

        #region 命令

        /// <summary>
        /// 打开
        /// </summary>
        public IAsyncRelayCommand On => p_On ??= new AsyncRelayCommand(OnAsync);
        IAsyncRelayCommand? p_On;
        public async Task OnAsync()
        {
            ICommunication instance = communication ?? throw new InvalidOperationException("Communication has not been initialized.");
            var result = await instance.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message ?? string.Empty);
            if (result.Status)
            {
                instance.OnInfoEventAsync -= Communication_OnInfoEventAsync;
                instance.OnInfoEventAsync += Communication_OnInfoEventAsync;
                instance.OnDataEventAsync -= Communication_OnDataEventAsync;
                instance.OnDataEventAsync += Communication_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public IAsyncRelayCommand Off => p_Off ??= new AsyncRelayCommand(OffAsync);
        IAsyncRelayCommand? p_Off;
        public async Task OffAsync()
        {
            if (communication == null)
            {
                return;  // 未启动，无需关闭
            }
            var result = await communication.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message ?? string.Empty);
            if (result.Status)
            {
                communication.OnInfoEventAsync -= Communication_OnInfoEventAsync;
                communication.OnDataEventAsync -= Communication_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await communication.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 发送
        /// </summary>
        public IAsyncRelayCommand Send => p_Send ??= new AsyncRelayCommand(SendAsync);
        IAsyncRelayCommand? p_Send;
        public async Task SendAsync()
        {
            string data = Data ?? string.Empty;
            if (data.IsNullOrWhiteSpace())
            {
                await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("数据不能为空") ?? "数据不能为空");
                return;
            }
            //发送的数据
            byte[] sendData;
            if (DataFormat.Equals(0))
            {
                sendData = Encoding.ASCII.GetBytes(data);
            }
            else
            {
                if (data.IsHexadecimal())
                {
                    sendData = data.ToHex(false);
                }
                else
                {
                    await uiMessage_InfoEvent.ShowAsync($"“{Data}”{App.LanguageOperate.GetLanguageValue("不是有效的 Hex 数据") ?? "不是有效的 Hex 数据"}");
                    return;
                }
            }

            ICommunication instance = communication ?? throw new InvalidOperationException("Communication has not been initialized.");
            await uiMessage_InfoEvent.ShowAsync((await instance.SendAsync(sendData)).Message ?? string.Empty);

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 发送等待
        /// </summary>
        public IAsyncRelayCommand SendWait => p_SendWait ??= new AsyncRelayCommand(SendWaitAsync);
        IAsyncRelayCommand? p_SendWait;
        public async Task SendWaitAsync()
        {
            string data = Data ?? string.Empty;
            if (data.IsNullOrWhiteSpace())
            {
                await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("数据不能为空") ?? "数据不能为空");
                return;
            }
            //发送的数据
            byte[] sendData;
            if (DataFormat.Equals(0))
            {
                sendData = Encoding.ASCII.GetBytes(data);
            }
            else
            {
                if (data.IsHexadecimal())
                {
                    sendData = data.ToHex(false);
                }
                else
                {
                    await uiMessage_InfoEvent.ShowAsync($"“{Data}”{App.LanguageOperate.GetLanguageValue("不是有效的 Hex 数据")}");
                    return;
                }
            }

            ICommunication instance = communication ?? throw new InvalidOperationException("Communication has not been initialized.");
            await uiMessage_InfoEvent.ShowAsync((await instance.SendWaitAsync(sendData, CancellationToken.None)).ToJson(true));

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 信息清空
        /// </summary>
        public IAsyncRelayCommand InfoClear => p_InfoClear ??= new AsyncRelayCommand(InfoClearAsync);
        IAsyncRelayCommand? p_InfoClear;
        public async Task InfoClearAsync()
        {
            await uiMessage_InfoEvent.ClearAsync();
        }

        /// <summary>
        /// 数据清空
        /// </summary>
        public IAsyncRelayCommand DataClear => p_DataClear ??= new AsyncRelayCommand(DataClearAsync);
        IAsyncRelayCommand? p_DataClear;
        public async Task DataClearAsync()
        {
            await uiMessage_DataEvent.ClearAsync();
        }
        #endregion

        #region 事件
        private async Task Communication_OnDataEventAsync(object? sender, EventDataResult e)
        {
            string addr = string.Empty;
            dynamic person = BasicsData;
            if (tag == "WsClient")
            {
                addr = $"{person.Host}";
            }
            else if (tag == "TcpClient" || tag == "UdpClient")
            {
                addr = $"{person.IpAddress}:{person.Port}";
            }
            else
            {
                addr = $"{person.Port}";
            }
            await uiMessage_DataEvent.ShowAsync((e.Message ?? string.Empty).Replace("[", "[ ").Replace("]", " ] "));
            if (e.ResultData != null && e.ResultData is byte[])
            {
                if (DataFormat == 0)
                {
                    await uiMessage_DataEvent.ShowAsync($"[ {addr} ] -> {Encoding.ASCII.GetString((byte[])e.ResultData)}");
                }
                else
                {
                    await uiMessage_DataEvent.ShowAsync($"[ {addr} ] -> {((byte[])e.ResultData).ToHexString()}");
                }
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
            uiMessage_DataEvent.OnInfoEventAsync += (object? sender, Model.data.EventInfoResult e) => { DataEvent = e.Message ?? string.Empty; return Task.CompletedTask; };
            await uiMessage_DataEvent.StartAsync();
            uiMessage_InfoEvent.OnInfoEventAsync += (object? sender, Model.data.EventInfoResult e) => { InfoEvent = e.Message ?? string.Empty; return Task.CompletedTask; };
            await uiMessage_InfoEvent.StartAsync();

            Core.handler.LanguageHandler.OnLanguageEventAsync -= LanguageHandler_OnLanguageEventAsync;
            Core.handler.LanguageHandler.OnLanguageEventAsync += LanguageHandler_OnLanguageEventAsync;
        }

        private async Task LanguageHandler_OnLanguageEventAsync(object? sender, EventLanguageResult? e)
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
                case "Tcp":
                    {
                        var obj = new TcpClientData.Basics();
                        communication = new TcpClientOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "Ws":
                    {
                        var obj = new WsClientData.Basics();
                        communication = new WsClientOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "UdpClient":
                    {
                        var obj = new UdpClientData.Basics();
                        communication = new UdpClientOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "UdpBroadcast":
                    {
                        var obj = new UdpBroadcastData.Basics();
                        communication = new UdpBroadcastOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "UdpMulticast":
                    {
                        var obj = new UdpMulticastData.Basics();
                        communication = new UdpMulticastOperate(obj);
                        BasicsData = obj;
                        break;
                    }
                case "Serial":
                    {
                        var obj = new SerialData.Basics();
                        communication = new SerialOperate(obj);
                        BasicsData = obj;
                        break;
                    }
            }
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            Core.handler.LanguageHandler.OnLanguageEventAsync -= LanguageHandler_OnLanguageEventAsync;
            communication?.Dispose();
            uiMessage_DataEvent.Dispose();
            uiMessage_InfoEvent.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            Core.handler.LanguageHandler.OnLanguageEventAsync -= LanguageHandler_OnLanguageEventAsync;
            if (communication != null) await communication.DisposeAsync();
            await uiMessage_DataEvent.DisposeAsync();
            await uiMessage_InfoEvent.DisposeAsync();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
