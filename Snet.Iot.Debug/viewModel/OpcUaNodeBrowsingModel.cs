using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Opc.Ua;
using Snet.Core.handler;
using Snet.Iot.Debug.handler;
using Snet.Iot.Debug.model;
using Snet.Model.data;
using Snet.Opc.core;
using Snet.Opc.ua.client;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Controls.property;
using Snet.Windows.Core.mvvm;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Snet.Iot.Debug.viewModel
{
    public class OpcUaNodeBrowsingModel : BindNotify, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// DAQ对象
        /// </summary>
        public OpcUaClientOperate Daq;

        /// <summary>
        /// 属性弹窗
        /// </summary>
        public PropertyControl param = new PropertyControl() { ButtonVisibility = Visibility.Visible };

        /// <summary>
        /// 树节点集合
        /// </summary>
        public ObservableCollection<OpcUaNodeBrowseStructuralBody> Node
        {
            get => GetProperty(() => Node);
            set => SetProperty(() => Node, value);
        }

        /// <summary>
        /// 树节点选中集合
        /// </summary>
        public OpcUaNodeBrowseStructuralBody NodeSelectedItem
        {
            get => GetProperty(() => NodeSelectedItem);
            set => SetProperty(() => NodeSelectedItem, value);
        }
        /// <summary>
        /// 节点信息集合
        /// </summary>
        public ObservableCollection<OpcUaNodeBrowseMessageStructuralBody> NodeMessage
        {
            get => GetProperty(() => NodeMessage);
            set => SetProperty(() => NodeMessage, value);
        }

        /// <summary>
        /// 选中的节点信息
        /// </summary>
        public OpcUaNodeBrowseMessageStructuralBody NodeMessageSelectedItem
        {
            get => GetProperty(() => NodeMessageSelectedItem);
            set => SetProperty(() => NodeMessageSelectedItem, value);
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
        /// 打开
        /// </summary>
        public IAsyncRelayCommand On => p_On ??= new AsyncRelayCommand(OnAsync);
        IAsyncRelayCommand? p_On;
        public async Task OnAsync()
        {
            if (Daq == null)
            {
                param.SetBasics(new OpcUaClientData.Basics());
                if ((await DialogHost.Show(param, "DialogHost")).ToBool())
                {
                    OpcUaClientData.Basics basics = param.GetBasics().GetSource<OpcUaClientData.Basics>();
                    Daq = await OpcUaClientOperate.InstanceAsync(basics);
                    OperateResult result = await Daq.OnAsync();
                    if (result.Status)
                    {
                        browseToken ??= new CancellationTokenSource();
                        //实例化对象
                        Node = new ObservableCollection<OpcUaNodeBrowseStructuralBody>();
                        //获取所有节点
                        await GetNodeInformAsync(browseToken.Token);
                    }
                    else
                    {
                        await OffAsync();
                        await Snet.Windows.Controls.message.MessageBox.Show(result.Message);
                    }
                }
                else
                {
                    await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("已取消"));
                }
            }
            else
            {
                await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("连接已经打开，如需更换请先关闭此连接"));
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public IAsyncRelayCommand Off => p_Off ??= new AsyncRelayCommand(OffAsync);
        IAsyncRelayCommand? p_Off;
        public async Task OffAsync()
        {
            if (Daq != null)
            {
                await Daq.OffAsync();
                await Daq.DisposeAsync();
            }
            browseToken?.Cancel();
            browseToken?.Dispose();
            browseToken = null;
            Node?.Clear();
            NodeMessage?.Clear();
            Daq = null;
        }

        #region 节点浏览
        /// <summary>
        /// 生命周期：浏览节点
        /// </summary>
        CancellationTokenSource browseToken;
        /// <summary>
        /// 选中的项
        /// </summary>
        private OpcUaNodeBrowseStructuralBody IsSelectItem;
        // 树节点选中后触发：显示当前节点及子节点的详细信息
        public IAsyncRelayCommand TreeView_SelectedItemChanged => p_TreeView_SelectedItemChanged ??= new AsyncRelayCommand<RoutedPropertyChangedEventArgs<object>>(TreeView_SelectedItemChangedAsync);
        IAsyncRelayCommand p_TreeView_SelectedItemChanged;
        public async Task TreeView_SelectedItemChangedAsync(RoutedPropertyChangedEventArgs<object>? e)
        {
            if (e?.NewValue is not OpcUaNodeBrowseStructuralBody selectedNode) return;

            if (selectedNode.NodeID is not ReferenceDescription reference) return;

            IsSelectItem = selectedNode;

            NodeId nodeId = (NodeId)reference.NodeId;

            if (selectedNode.Children.Count == 1 && string.IsNullOrWhiteSpace(selectedNode.Children[0].Name))
            {
                selectedNode.Children.Clear();
                await GetNodeInformAsync(nodeId, selectedNode, browseToken.Token, false);
            }

            Address = nodeId.ToString();
            var nodeIds = new List<NodeId>();

            if (selectedNode.Children.Count > 0)
            {
                foreach (var child in selectedNode.Children)
                {
                    if (child?.NodeID is ReferenceDescription childRef)
                        nodeIds.Add((NodeId)childRef.NodeId);
                }
            }
            else
            {
                nodeIds.Add(nodeId);
            }

            var dataValues = Daq.DetailedReadAllNodeData(nodeIds).GetSource<DataValue[]>();
            if (dataValues != null)
            {
                await ShowDetailedMessageAsync(nodeId.ToString(), dataValues, nodeIds, selectedNode.IsLoading);
            }
        }

        // 表格被选中时更新地址绑定
        public IAsyncRelayCommand DataGrid_SelectedCellsChanged => p_DataGrid_SelectedCellsChanged ??= new AsyncRelayCommand<object>(DataGrid_SelectedCellsChangedAsync);
        IAsyncRelayCommand p_DataGrid_SelectedCellsChanged;
        public Task DataGrid_SelectedCellsChangedAsync(object? e)
        {
            if (!string.IsNullOrWhiteSpace(NodeMessageSelectedItem?.Name))
                Address = NodeMessageSelectedItem.Address;

            return Task.CompletedTask;
        }

        // TreeView 节点展开时触发动态加载
        public IAsyncRelayCommand TreeViewItem_Expanded => p_TreeViewItem_Expanded ??= new AsyncRelayCommand<RoutedEventArgs>(TreeViewItem_ExpandedAsync);
        IAsyncRelayCommand p_TreeViewItem_Expanded;
        private async Task TreeViewItem_ExpandedAsync(RoutedEventArgs? e)
        {
            if (e?.OriginalSource is TreeViewItem item && item.DataContext is OpcUaNodeBrowseStructuralBody node)
            {
                if (node.PageIndex > 1) return;
                if (node.NodeID is ReferenceDescription reference && node.Children.Count == 1 && string.IsNullOrWhiteSpace(node.Children[0].Name))
                {
                    IsSelectItem = node;
                    node.Children.Clear();
                    await GetNodeInformAsync((NodeId)reference.NodeId, node, browseToken.Token, false);
                }
            }
        }

        // 分割 DataValue[] 为指定列宽度
        private async Task<List<object[]>> SegmentationAsync(object[] data, int segmentSize)
        {
            var result = new List<object[]>();
            for (int i = 0; i < data.Length; i += segmentSize)
            {
                var segment = new object[segmentSize];
                for (int j = 0; j < segmentSize && i + j < data.Length; j++)
                    segment[j] = data[i + j];

                result.Add(segment);
            }
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        // 缓存 NodeId 的字典
        ConcurrentDictionary<string, List<OpcUaNodeBrowseMessageStructuralBody>> _cacheData = new ConcurrentDictionary<string, List<OpcUaNodeBrowseMessageStructuralBody>>();
        // 显示当前节点详细值、数据类型、描述、访问权限等
        private async Task ShowDetailedMessageAsync(string upNodeId, DataValue[] dataValues, List<NodeId> nodeIds, bool isLoading)
        {
            await Task.Run(async () =>
            {
                //实例化
                List<OpcUaNodeBrowseMessageStructuralBody>? nodeMessage = null;
                if (!_cacheData.TryGetValue(upNodeId, out nodeMessage) || !isLoading)
                {
                    nodeMessage ??= new List<OpcUaNodeBrowseMessageStructuralBody>();
                    nodeMessage.Clear();
                    var segments = await SegmentationAsync(dataValues, 5);
                    for (int i = 0; i < nodeIds.Count && i < segments.Count; i++)
                    {
                        var segment = segments[i];

                        var data = new OpcUaNodeBrowseMessageStructuralBody
                        {
                            Name = nodeIds[i].Identifier.ToString(),
                            Address = nodeIds[i].ToString(),
                            Value = segment[1]?.ToString(),
                            Type = Daq.GetNodeValueType(nodeIds[i]).GetSource<BuiltInType>().ToString(),
                            Description = segment[4]?.ToString(),
                            AccessLevel = Daq.GetAccessLevel((DataValue)segment[2])
                        };
                        nodeMessage.Add(data);
                    }
                    _cacheData.AddOrUpdate(upNodeId, nodeMessage, (o, v) => nodeMessage);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NodeMessage = new ObservableCollection<OpcUaNodeBrowseMessageStructuralBody>(nodeMessage);
                });
            }, browseToken.Token);
        }

        // 缓存字典
        private readonly ConcurrentDictionary<string, DrawingImage> _iconCache = new();
        /// <summary>
        /// 获取图标资源，如果不存在则返回默认图标
        /// </summary>
        /// <param name="iconKey">资源名</param>
        /// <param name="defaultKey">默认图标资源名</param>
        /// <returns>DrawingImage 图标</returns>
        public async Task<DrawingImage> GetIcon(string iconKey, string defaultKey = "Default")
        {
            // 已缓存
            if (_iconCache.TryGetValue(iconKey, out var cached))
                return cached;
            else
                _iconCache[iconKey] = Application.Current.FindResource(iconKey).GetSource<DrawingImage>();
            return _iconCache[iconKey];
        }

        public T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                if (parentObject is T parent)
                    return parent;

                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null;
        }
        /// <summary>
        /// 滚动条到底部时加载更多节点
        /// </summary>
        public IAsyncRelayCommand ScrollViewer_ScrollChanged => p_ScrollViewer_ScrollChanged ??= new AsyncRelayCommand<ScrollChangedEventArgs>(ScrollViewer_ScrollChangedAsync);
        IAsyncRelayCommand p_ScrollViewer_ScrollChanged;
        private async Task ScrollViewer_ScrollChangedAsync(ScrollChangedEventArgs? e)
        {
            if (e == null) return;

            // 若未发生滚动变化（如 TreeViewItem 展开引起的），则跳过
            if (e.VerticalChange <= 0)
                return;

            if (e.VerticalOffset >= e.ExtentHeight - e.ViewportHeight - 50)
            {
                if (IsSelectItem == null) return;
                if (IsSelectItem is OpcUaNodeBrowseStructuralBody selected)
                {
                    // 加载当前节点更多
                    await GetNodeInformAsync((NodeId)IsSelectItem.NodeID.GetSource<ReferenceDescription>().NodeId, selected, CancellationToken.None, true);
                }
            }
        }

        /// <summary>
        /// 第一次加载
        /// </summary>
        private async Task GetNodeInformAsync(CancellationToken token)
        {
            var id = Daq.GetNodeID().GetSource<NodeId>();
            ReferenceDescriptionCollection references = (await Daq.GetAllNode(id)).GetSource<ReferenceDescriptionCollection>();
            foreach (var reference in references)
            {
                if (token.IsCancellationRequested) return; // 取消操作

                var childRefs = (await Daq.GetAllNode((NodeId)reference.NodeId)).GetSource<ReferenceDescriptionCollection>();

                if (childRefs != null)
                {
                    if (token.IsCancellationRequested)
                        return;

                    var iconName = await Daq.GetNodeIconType(reference, id);

                    var body = new OpcUaNodeBrowseStructuralBody
                    {
                        Name = reference.BrowseName.Name,
                        NodeID = reference,
                        Icon = await GetIcon(iconName),
                        IconKey = iconName,
                        Count = childRefs.Count > 0 ? $"( {childRefs.Count} )" : string.Empty
                    };
                    if (childRefs.Count > 0)
                        body.Children.Add(new());
                    await Application.Current.Dispatcher.InvokeAsync(() => Node.Add(body));
                }
            }
        }

        // 递归获取指定节点的子节点集合，并构建树结构
        private async Task GetNodeInformAsync(NodeId nodeId, OpcUaNodeBrowseStructuralBody parent, CancellationToken token, bool theScrollbarIsAtTheBottom)
        {
            try
            {
                if (parent.IsLoading) return;
                var id = Daq.GetNodeID().GetSource<NodeId>();
                ReferenceDescriptionCollection references = (await Daq.GetAllNode(nodeId)).GetSource<ReferenceDescriptionCollection>();
                PagedResult<ReferenceDescription> result = PageHandler.ToPagedResult(references, parent.PageIndex);
                if (references.Count > result.PageSize && !result.IsLastPage)
                {
                    parent.PageIndex = result.PageIndex + 1;
                }
                else
                {
                    parent.PageIndex = 1;
                    parent.IsLoading = result.IsLastPage;
                }
                foreach (var reference in result.Items)
                {
                    if (token.IsCancellationRequested) return; // 取消操作

                    var childRefs = (await Daq.GetAllNode((NodeId)reference.NodeId)).GetSource<ReferenceDescriptionCollection>();

                    if (childRefs != null)
                    {

                        var iconName = await Daq.GetNodeIconType(reference, id);

                        var body = new OpcUaNodeBrowseStructuralBody
                        {
                            Name = reference.BrowseName.Name,
                            NodeID = reference,
                            Icon = await GetIcon(iconName),
                            IconKey = iconName,
                            Count = childRefs.Count > 0 ? $"( {childRefs.Count} )" : string.Empty
                        };
                        if (childRefs.Count > 0)
                            body.Children.Add(new());
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            if (parent == null)
                                Node.Add(body);
                            else
                                parent.Children.Add(body);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await Snet.Windows.Controls.message.MessageBox.Show($"{App.LanguageOperate.GetLanguageValue("节点加载失败")}：{ex.Message}");
            }
        }

        // 鼠标右键 TreeView 上点击，确保节点被选中
        public IAsyncRelayCommand TreeView_PreviewMouseRightButtonDown => p_TreeView_PreviewMouseRightButtonDown ??= new AsyncRelayCommand<MouseButtonEventArgs>(TreeView_PreviewMouseRightButtonDownAsync);
        IAsyncRelayCommand p_TreeView_PreviewMouseRightButtonDown;
        public Task TreeView_PreviewMouseRightButtonDownAsync(MouseButtonEventArgs? e)
        {
            if (e?.OriginalSource is DependencyObject dep)
            {
                var treeItem = VisualUpwardSearch<TreeViewItem>(dep) as TreeViewItem;
                NodeSelectedItem = treeItem?.DataContext as OpcUaNodeBrowseStructuralBody;
            }
            return Task.CompletedTask;
        }

        // 递归查找指定类型的父控件
        private DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);
            return source;
        }

        // 导出当前节点及其子节点结构到 json
        public IAsyncRelayCommand ContextMenu_ExpNode => p_ContextMenu_ExpNode ??= new AsyncRelayCommand<object>(ContextMenu_ExpNodeAsync);
        IAsyncRelayCommand p_ContextMenu_ExpNode;
        public async Task ContextMenu_ExpNodeAsync(object? e)
        {
            if (NodeSelectedItem == null)
            {
                await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("请选中节点后操作"));
                return;
            }

            string path = Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件夹"), true);
            if (string.IsNullOrEmpty(path))
            {
                await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("取消节点导出，未选择存储路径"));
                return;
            }

            var jsonRoot = await ExpNodesAsync(NodeSelectedItem);
            if (jsonRoot == null)
            {
                await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("请把所有子节点都展开在进行导出操作"));
                return;
            }

            string baseName = jsonRoot.Name;
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffffff");

            string pathTree = $"{path}\\[{baseName}]Node {timeStamp}.json";
            FileHandler.StringToFile(pathTree, JsonConvert.SerializeObject(jsonRoot, Formatting.Indented));

            await ecAsync(jsonRoot);

            Address addressList = new()
            {
                AddressArray = nodes.ConvertAll(n => new AddressDetails
                {
                    AddressName = n.Address,
                    AddressDescribe = n.Description,
                    AddressDataType = Daq.TypeConvert((BuiltInType)Enum.Parse(typeof(BuiltInType), n.DataType))
                })
            };

            string pathAddress = $"{path}\\[{baseName}]Node_Address {timeStamp}.json";
            FileHandler.StringToFile(pathAddress, addressList.ToJson(true));

            await Snet.Windows.Controls.message.MessageBox.Show($"{App.LanguageOperate.GetLanguageValue("节点成功导出至")}：{pathAddress}");
        }

        // 辅助递归导出子节点数据结构
        private async Task<NodeBody> ExpNodesAsync(OpcUaNodeBrowseStructuralBody node, NodeBody nodeJson = null)
        {
            if (node.NodeID is not ReferenceDescription refDesc) return null;

            nodeJson ??= new NodeBody();
            NodeId nodeId = (NodeId)refDesc.NodeId;

            var values = Daq.DetailedReadAllNodeData(new List<NodeId> { nodeId }).GetSource<DataValue[]>();

            nodeJson.DataType = Daq.GetNodeValueType(nodeId).GetSource<BuiltInType>().ToString();
            nodeJson.Name = values[3]?.ToString();
            nodeJson.Description = values[4]?.ToString();

            if (node.Children.Count > 0)
            {
                nodeJson.Nodes = new();
                foreach (var child in node.Children)
                {
                    var childJson = await ExpNodesAsync(child);
                    if (childJson != null) nodeJson.Nodes.Add(childJson);
                }
            }
            else
            {
                nodeJson.Address = nodeId.ToString();
                nodeJson.Name = refDesc.ToString();
            }
            return nodeJson;
        }

        private List<NodeBody> nodes;
        private async Task ecAsync(NodeBody node)
        {
            nodes ??= new();

            if (node.Nodes != null)
            {
                foreach (var sub in node.Nodes)
                    await ecAsync(sub);
            }
            else
            {
                nodes.Add(node);
            }
        }

        // 订阅选中节点
        public IAsyncRelayCommand ContextMenu_Subscribe => p_ContextMenu_Subscribe ??= new AsyncRelayCommand<object>(ContextMenu_SubscribeAsync);
        IAsyncRelayCommand p_ContextMenu_Subscribe;
        public async Task ContextMenu_SubscribeAsync(object? e)
        {
            if (NodeSelectedItem == null)
            {
                await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("请选中节点后操作"));
                return;
            }

            var address = new Address
            {
                AddressArray = SubscribeNodes(NodeSelectedItem).ConvertAll(a => new AddressDetails { AddressName = a })
            };

            var result = await Daq.SubscribeAsync(address);
            await Snet.Windows.Controls.message.MessageBox.Show(result.ToJson(true));
        }

        // 取消订阅
        public IAsyncRelayCommand ContextMenu_UnSubscribe => p_ContextMenu_UnSubscribe ??= new AsyncRelayCommand<object>(ContextMenu_UnSubscribeAsync);
        IAsyncRelayCommand p_ContextMenu_UnSubscribe;
        public async Task ContextMenu_UnSubscribeAsync(object? e)
        {
            if (NodeSelectedItem == null)
            {
                await Snet.Windows.Controls.message.MessageBox.Show(App.LanguageOperate.GetLanguageValue("请选中节点后操作"));
                return;
            }

            var address = new Address
            {
                AddressArray = SubscribeNodes(NodeSelectedItem)
                    .ConvertAll(a => new AddressDetails { AddressName = a })
            };

            var result = await Daq.UnSubscribeAsync(address);
            await Snet.Windows.Controls.message.MessageBox.Show(result.ToJson(true));
        }

        // 获取节点中所有末端节点地址
        private List<string> SubscribeNodes(OpcUaNodeBrowseStructuralBody node, List<string> result = null)
        {
            result ??= new();
            if (node.Children.Count == 0)
            {
                var value = (node.NodeID as ReferenceDescription)?.NodeId.ToString();
                if (!value.IsNullOrWhiteSpace())
                {
                    result.Add(value);
                }
            }
            else
            {
                foreach (var child in node.Children)
                {
                    result.AddRange(SubscribeNodes(child));
                }
            }
            return result;
        }
        #endregion


        public void Dispose()
        {
            try
            {
                Daq?.Dispose();
            }
            catch { }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (Daq != null)
                {
                    await Daq.DisposeAsync();
                }
            }
            catch { }
        }
    }
}
