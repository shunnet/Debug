using Snet.Core.handler;
using Snet.Utility;
using Snet.Windows.Core.mvvm;
using System.Windows.Controls;

namespace Snet.Iot.Debug.model
{
    /// <summary>表示一个可关闭的调试页签，并拥有其内容数据上下文的生命周期。</summary>
    public sealed class TabControlDeviceModel : BindNotify, IDisposable, IAsyncDisposable
    {
        private int disposed;
        /// <summary>
        /// 构造函数，初始化设备详情和内容并刷新显示数据
        /// </summary>
        public TabControlDeviceModel(string nameKey, UserControl content)
        {
            Content = content;
            NameKey = nameKey;
            Header = App.LanguageOperate.GetLanguageValue(NameKey) ?? NameKey;
            Core.handler.LanguageHandler.OnLanguageEvent += LanguageHandler_OnLanguageEvent;
        }

        /// <summary>
        /// 语言发生变化
        /// </summary>
        private void LanguageHandler_OnLanguageEvent(object? sender, Model.data.EventLanguageResult e)
        {
            Header = App.LanguageOperate.GetLanguageValue(NameKey) ?? NameKey;
        }

        /// <summary>
        /// 名称键值
        /// </summary>
        public string NameKey;
        /// <summary>
        /// 头文本
        /// </summary>
        public string Header
        {
            get => GetProperty(() => Header);
            set => SetProperty(() => Header, value);
        }

        /// <summary>
        /// 内容
        /// </summary>
        public UserControl Content
        {
            get => GetProperty(() => Content);
            set => SetProperty(() => Content, value);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
                return;
            // 退订静态语言事件，避免关闭 Tab 后本模型被静态事件持有（泄漏）
            Core.handler.LanguageHandler.OnLanguageEvent -= LanguageHandler_OnLanguageEvent;
            (Content.DataContext as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 异步释放
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
                return;
            // 退订静态语言事件，避免关闭 Tab 后本模型被静态事件持有（泄漏）
            Core.handler.LanguageHandler.OnLanguageEvent -= LanguageHandler_OnLanguageEvent;
            if (Content.DataContext is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                (Content.DataContext as IDisposable)?.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
