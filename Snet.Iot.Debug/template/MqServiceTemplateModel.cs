using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit;
using Snet.Core.handler;
using Snet.Model.data;
using Snet.Model.@enum;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.data;
using Snet.Windows.Core.mvvm;
using System.Windows.Input;

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

        /// <summary>
        /// 拦截键盘按键，阻止粘贴（Ctrl+V）、删除和退格操作
        /// </summary>
        public IAsyncRelayCommand TextEditor_PreviewKeyDown => p_TextEditor_PreviewKeyDown ??= new AsyncRelayCommand<EventCommandArgs>(TextEditor_PreviewKeyDownAsync);
        IAsyncRelayCommand? p_TextEditor_PreviewKeyDown;
        public async Task TextEditor_PreviewKeyDownAsync(EventCommandArgs? e)
        {
            KeyEventArgs keyEvent = e.EventArgs.GetSource<KeyEventArgs>();
            if ((keyEvent.Key == System.Windows.Input.Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) || keyEvent.Key == System.Windows.Input.Key.Delete || keyEvent.Key == System.Windows.Input.Key.Back)
            {
                keyEvent.Handled = true;
            }
        }

        /// <summary>
        /// 拦截文本输入，防止用户手动编辑日志内容
        /// </summary>
        public IAsyncRelayCommand TextEditor_PreviewTextInput => p_TextEditor_PreviewTextInput ??= new AsyncRelayCommand<EventCommandArgs>(TextEditor_PreviewTextInputAsync);
        IAsyncRelayCommand? p_TextEditor_PreviewTextInput;
        public async Task TextEditor_PreviewTextInputAsync(EventCommandArgs? e)
        {
            TextCompositionEventArgs eventArgs = e.EventArgs.GetSource<TextCompositionEventArgs>();
            eventArgs.Handled = true;
        }


        /// <summary>
        /// 文本内容变化时自动滚动到末尾，保持最新日志可见
        /// </summary>
        public IAsyncRelayCommand TextEditor_TextChanged => p_TextEditor_TextChanged ??= new AsyncRelayCommand<EventCommandArgs>(TextEditor_TextChangedAsync);
        IAsyncRelayCommand p_TextEditor_TextChanged;
        public async Task TextEditor_TextChangedAsync(EventCommandArgs? e)
        {
            TextEditor text = e.Source.GetSource<TextEditor>();
            text.SelectionStart = text.Text.Length;
            text.SelectionLength = 0;
            text.ScrollToEnd();
        }



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
