using CommunityToolkit.Mvvm.Input;
using Snet.Core.handler;
using Snet.Model.data;
using Snet.Model.@enum;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;

namespace Snet.Iot.Debug.template
{
    public class MqServiceTemplateModel<T> : BindNotify, IDisposable, IAsyncDisposable
    {
        public MqServiceTemplateModel()
        {
            // 界面消息处理
            uiMessage_DataEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => DataEvent = e.Message;
            uiMessage_DataEvent.StartAsync();
            uiMessage_InfoEvent.OnInfoEventAsync += async (object? sender, Model.data.EventInfoResult e) => InfoEvent = e.Message;
            uiMessage_InfoEvent.StartAsync();

            Core.handler.LanguageHandler.OnLanguageEventAsync -= LanguageHandler_OnLanguageEventAsync;
            Core.handler.LanguageHandler.OnLanguageEventAsync += LanguageHandler_OnLanguageEventAsync;

        }

        public async Task LanguageHandler_OnLanguageEventAsync(object? sender, EventLanguageResult e)
        {
            string title = (await Core.handler.LanguageHandler.GetLanguageAsync()) == LanguageType.zh ? " 调试工具" : " Debug Tool";
            ToolTitle = (await App.LanguageOperate.GetLanguageValueAsync(Key)) + title;
        }


        /// <summary>
        /// Mq服务端对象
        /// </summary>
        public object MqService { get; set; }

        /// <summary>
        /// 多语言的健
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 基础数据
        /// </summary>
        public T BasicsData
        {
            get => GetProperty(() => BasicsData);
            set => SetProperty(() => BasicsData, value);
        }

        /// <summary>
        /// ui信息处理器
        /// </summary>
        public UiMessageHandler uiMessage_InfoEvent = new UiMessageHandler("InfoEvent");
        public UiMessageHandler uiMessage_DataEvent = new UiMessageHandler("DataEvent");


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
        public virtual async Task OnAsync() { }

        /// <summary>
        /// 关闭
        /// </summary>
        public IAsyncRelayCommand Off => p_Off ??= new AsyncRelayCommand(OffAsync);
        IAsyncRelayCommand p_Off;
        public virtual async Task OffAsync() { }

        #region 事件
        public async Task Mq_OnDataEventAsync(object? sender, EventDataResult e)
        {
            string msg = e.ToJson(true);
            if (msg.IsNullOrWhiteSpace())
                return;
            await uiMessage_DataEvent.ShowAsync($" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} : {msg}\r\n");
        }

        public async Task Mq_OnInfoEventAsync(object? sender, EventInfoResult e)
        {
            string msg = e.ToJson(true);
            if (msg.IsNullOrWhiteSpace())
                return;
            await uiMessage_InfoEvent.ShowAsync($" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} : {msg}\r\n");
        }

        #endregion





        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            try
            {
                IDisposable? sisposable = MqService.GetSource<IDisposable>();
                if (sisposable != null)
                {
                    sisposable.Dispose();
                }
            }
            catch { }
        }
        /// <summary>
        /// 异步释放
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            try
            {
                IAsyncDisposable? asyncDisposable = MqService.GetSource<IAsyncDisposable>();
                if (asyncDisposable != null)
                {
                    await asyncDisposable.DisposeAsync();
                }
            }
            catch { }
        }







    }
}
