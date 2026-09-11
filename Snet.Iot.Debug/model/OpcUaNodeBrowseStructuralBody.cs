using Snet.Utility;
using Snet.Windows.Core.handler;
using Snet.Windows.Core.mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Snet.Iot.Debug.model
{
    /// <summary>
    /// OPC UA 节点浏览结构体，用于树形结构展示节点信息并支持分页加载。
    /// </summary>
    public sealed class OpcUaNodeBrowseStructuralBody : BindNotify, IDisposable
    {

        /// <summary>
        /// 索引位置记录
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// 节点图片名称（资源键），默认为空字符串以避免空引用。
        /// </summary>
        public string IconKey { get; set; } = string.Empty;

        /// <summary>
        /// 节点图片
        /// </summary>
        public object? Icon
        {
            get => GetProperty(() => Icon);
            set => SetProperty(() => Icon, value);
        }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name
        {
            get => GetProperty(() => Name);
            set => SetProperty(() => Name, value);
        }

        /// <summary>
        /// 节点对象
        /// </summary>
        public object? NodeID
        {
            get => GetProperty(() => NodeID);
            set => SetProperty(() => NodeID, value);
        }

        /// <summary>
        /// 数量
        /// </summary>
        public string Count
        {
            get => GetProperty(() => Count);
            set => SetProperty(() => Count, value);
        }

        /// <summary>当前节点已加载的直接子节点。</summary>
        public ObservableCollection<OpcUaNodeBrowseStructuralBody> Children
        {
            get => GetProperty(() => Children);
            set => SetProperty(() => Children, value);
        }

        public OpcUaNodeBrowseStructuralBody()
        {
            Children = new ObservableCollection<OpcUaNodeBrowseStructuralBody>();
            Name = string.Empty;
            Count = string.Empty;
            SkinHandler.OnSkinEventAsync += SkinHandler_OnSkinEventAsync;
        }

        /// <summary>主题变化后在 UI 线程刷新当前节点图标。</summary>
        private async Task SkinHandler_OnSkinEventAsync(object? sender, Windows.Core.data.EventSkinResult e)
        {
            Application? application = Application.Current;
            if (application is null || IconKey.IsNullOrWhiteSpace())
                return;

            await application.Dispatcher.InvokeAsync(() =>
            {
                if (application.TryFindResource(IconKey) is DrawingImage image)
                    Icon = image;
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private bool disposed;

        /// <summary>
        /// 释放节点：退订静态皮肤事件并递归释放子节点。<br/>
        /// 注意：SkinHandler.OnSkinEventAsync 是静态事件，每个节点的订阅都必须显式退订，
        /// 否则节点被清空后仍被静态事件持有（内存泄漏，且每次皮肤切换会唤醒全部陈旧节点）。
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            SkinHandler.OnSkinEventAsync -= SkinHandler_OnSkinEventAsync;
            foreach (var child in Children)
            {
                child?.Dispose();
            }
            Children.Clear();
        }
    }
}
