using CommunityToolkit.Mvvm.Input;
using Snet.Core.handler;
using Snet.Kafka;
using Snet.Model.data;
using Snet.Model.@enum;
using Snet.Model.@interface;
using Snet.Mqtt.client;
using Snet.NetMQ;
using Snet.Netty.client;
using Snet.RabbitMQ;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;

namespace Snet.Iot.Debug.viewModel
{
    public class MqModel : BindNotify, IDisposable, IAsyncDisposable
    {
        #region 属性
        /// <summary>
        /// 传输对象
        /// 外部设置好传进来
        /// </summary>
        private IMq mq;

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
        /// 主题
        /// </summary>
        public string Topic
        {
            get => GetProperty(() => Topic);
            set => SetProperty(() => Topic, value);
        }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get => GetProperty(() => Content);
            set => SetProperty(() => Content, value);
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
        /// 打开
        /// </summary>
        public IAsyncRelayCommand On => p_On ??= new AsyncRelayCommand(OnAsync);
        IAsyncRelayCommand p_On;
        public async Task OnAsync()
        {
            mq = (await mq.CreateInstanceAsync(BasicsData.ToJson(true))).ResultData.GetSource<IMq>();

            var result = await mq.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
            {
                mq.OnInfoEventAsync -= Mq_OnInfoEventAsync;
                mq.OnInfoEventAsync += Mq_OnInfoEventAsync;
                mq.OnDataEventAsync -= Mq_OnDataEventAsync;
                mq.OnDataEventAsync += Mq_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public IAsyncRelayCommand Off => p_Off ??= new AsyncRelayCommand(OffAsync);
        IAsyncRelayCommand p_Off;
        public async Task OffAsync()
        {
            var result = await mq.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
            if (result.Status)
            {
                mq.OnInfoEventAsync -= Mq_OnInfoEventAsync;
                mq.OnDataEventAsync -= Mq_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 发布
        /// </summary>
        public IAsyncRelayCommand Issue => p_Issue ??= new AsyncRelayCommand(IssueAsync);
        IAsyncRelayCommand p_Issue;
        public async Task IssueAsync()
        {
            await uiMessage_InfoEvent.ShowAsync((await mq.ProduceAsync(Topic, Content)).Message);
            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 订阅
        /// </summary>
        public IAsyncRelayCommand Subscribe => p_Subscribe ??= new AsyncRelayCommand(SubscribeAsync);
        IAsyncRelayCommand p_Subscribe;
        public async Task SubscribeAsync()
        {
            await uiMessage_InfoEvent.ShowAsync((await mq.ConsumeAsync(Topic)).Message);
            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 2;
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public IAsyncRelayCommand UnSubscribe => p_UnSubscribe ??= new AsyncRelayCommand(UnSubscribeAsync);
        IAsyncRelayCommand p_UnSubscribe;
        public async Task UnSubscribeAsync()
        {
            await uiMessage_InfoEvent.ShowAsync((await mq.UnConsumeAsync(Topic)).Message);
            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }
        #endregion

        #region 事件
        private async Task Mq_OnDataEventAsync(object? sender, EventDataResult e)
        {
            string msg = e.ToJson(true);
            if (msg.IsNullOrWhiteSpace())
                return;
            await uiMessage_DataEvent.ShowAsync($" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} : {msg}\r\n");
        }

        private async Task Mq_OnInfoEventAsync(object? sender, EventInfoResult e)
        {
            string msg = e.ToJson(true);
            if (msg.IsNullOrWhiteSpace())
                return;
            await uiMessage_InfoEvent.ShowAsync($" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} : {msg}\r\n");
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置对象
        /// </summary>
        public void SetObject(string tag)
        {
            LanguageHandler_OnLanguageEventAsync(null, null);
            InitBasicsData(tag);
            this.tag = tag;

            // 界面消息处理
            uiMessage_DataEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => DataEvent = e.Message;
            uiMessage_DataEvent.StartAsync();
            uiMessage_InfoEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InfoEvent = e.Message;
            uiMessage_InfoEvent.StartAsync();

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
                case "Mqtt":
                    mq = new MqttClientOperate();
                    BasicsData = new MqttClientData.Basics();
                    break;
                case "Netty":
                    mq = new NettyClientOperate();
                    BasicsData = new NettyClientData.Basics();
                    break;
                case "RabbitMQ":
                    mq = new RabbitMQOperate();
                    BasicsData = new RabbitMQData.Basics();
                    break;
                case "Kafka":
                    mq = new KafkaOperate();
                    BasicsData = new KafkaData.Basics();
                    break;
                case "NetMQ":
                    mq = new NetMQOperate();
                    BasicsData = new NetMQData.Basics();
                    break;
            }
        }
        public void Dispose()
        {
            try
            {
                mq?.Dispose();
            }
            catch { }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (mq != null)
                {
                    await mq.DisposeAsync();
                }
            }
            catch { }
        }
        #endregion
    }
}
