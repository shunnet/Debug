using Snet.Core.handler;
using Snet.Utility;
using Snet.Windows.Core.mvvm;
using System.Windows.Controls;

namespace Snet.Iot.Debug.model
{
    public class TabControlDeviceModel : BindNotify
    {
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
            IDisposable? sisposable = Content.DataContext.GetSource<IDisposable>();
            if (sisposable != null)
            {
                sisposable.Dispose();
            }
        }
        /// <summary>
        /// 异步释放
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable? asyncDisposable = Content.DataContext.GetSource<IAsyncDisposable>();
            if (asyncDisposable != null)
            {
                await asyncDisposable.DisposeAsync();
            }

        }
    }
}
