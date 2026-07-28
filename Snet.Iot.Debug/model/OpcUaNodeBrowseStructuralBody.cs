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
    public class OpcUaNodeBrowseStructuralBody : BindNotify
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
        public object Icon
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
        public object NodeID
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

        public ObservableCollection<OpcUaNodeBrowseStructuralBody> Children
        {
            get => GetProperty(() => Children);
            set => SetProperty(() => Children, value);
        }

        public OpcUaNodeBrowseStructuralBody()
        {
            Children = new ObservableCollection<OpcUaNodeBrowseStructuralBody>();
            SkinHandler.OnSkinEventAsync += SkinHandler_OnSkinEventAsync;
        }

        private Task SkinHandler_OnSkinEventAsync(object? sender, Windows.Core.data.EventSkinResult e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!IconKey.IsNullOrWhiteSpace())
                {
                    Icon = (DrawingImage)Application.Current.FindResource(IconKey);
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
            return Task.CompletedTask;
        }
    }
}
