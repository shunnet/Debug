using CommunityToolkit.Mvvm.Input;
using Opc.Ua;
using Snet.Core.handler;
using Snet.Model.data;
using Snet.Opc.core;
using Snet.Opc.ua.service;
using Snet.Utility;
using Snet.Windows.Controls.data;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;

namespace Snet.Iot.Debug.viewModel
{
    /// <summary>OPC UA 服务端调试、节点管理及导入模型。</summary>
    public sealed class OpcUaServiceModel : BindNotify, IDisposable, IAsyncDisposable
    {
        private readonly Task initializationTask;
        private int disposed;

        public OpcUaServiceModel()
        {
            // 界面消息处理
            uiMessage_DataEvent.OnInfoEventAsync += (object? sender, Model.data.EventInfoResult e) =>
            {
                DataEvent = e.Message ?? string.Empty;
                return Task.CompletedTask;
            };
            uiMessage_InfoEvent.OnInfoEventAsync += (object? sender, Model.data.EventInfoResult e) =>
            {
                InfoEvent = e.Message ?? string.Empty;
                return Task.CompletedTask;
            };
            initializationTask = InitializeMessageHandlersAsync();
        }

        private async Task InitializeMessageHandlersAsync()
        {
            await uiMessage_DataEvent.StartAsync();
            await uiMessage_InfoEvent.StartAsync();
        }

        /// <summary>
        /// ui信息处理器
        /// </summary>
        private readonly UiMessageHandler uiMessage_InfoEvent = new($"InfoEvent·{Guid.NewGuid()}");
        private readonly UiMessageHandler uiMessage_DataEvent = new($"DataEvent·{Guid.NewGuid()}");
        /// <summary>
        /// DAQ对象
        /// </summary>
        public OpcUaServiceOperate? Communication { get; set; }
        /// <summary>
        /// 导出的文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

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
        /// 基础数据
        /// </summary>
        public OpcUaServiceData.Basics BasicsData
        {
            get => basicsData;
            set => SetProperty(ref basicsData, value);
        }
        private OpcUaServiceData.Basics basicsData = new();

        /// <summary>
        /// 数据类型
        /// </summary>
        public int DataType
        {
            get => dataType;
            set => SetProperty(ref dataType, value);
        }
        private int dataType = 21;

        /// <summary>
        /// 节点地址
        /// </summary>
        public string DotAddress
        {
            get => GetProperty(() => DotAddress);
            set => SetProperty(() => DotAddress, value);
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        public string WriteInData
        {
            get => GetProperty(() => WriteInData);
            set => SetProperty(() => WriteInData, value);
        }

        /// <summary>
        /// 文件夹名称
        /// </summary>
        public string FolderName
        {
            get => GetProperty(() => FolderName);
            set => SetProperty(() => FolderName, value);
        }
        /// <summary>
        /// 下拉框数据源
        /// </summary>
        public ObservableCollection<ComboBoxModel> ComboBoxItemsSource
        {
            get => _ComboBoxItemsSource;
            set => SetProperty(ref _ComboBoxItemsSource, value);
        }
        private ObservableCollection<ComboBoxModel> _ComboBoxItemsSource = new ObservableCollection<ComboBoxModel>();

        /// <summary>
        /// 下拉框数选中的数据
        /// </summary>
        public ComboBoxModel? ComboBoxSelectedItem
        {
            get => GetProperty(() => ComboBoxSelectedItem);
            set => SetProperty(() => ComboBoxSelectedItem, value);
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



        private async Task Daq_OnDataEventAsync(object? sender, EventDataResult e)
        {
            if (e.GetDetails(out string? message, out ConcurrentDictionary<string, AddressValue>? data) && data is not null)
            {
                foreach (var item in data)
                {
                    switch (item.Value.AddressDataType)
                    {
                        case Model.@enum.DataType.ByteArray when item.Value.ResultValue is byte[] bytes:
                            await uiMessage_DataEvent.ShowAsync($"{e.Message}\r\n键：{item.Key}\r\n值：{ByteHandler.ByteToHexString(bytes, ' ')}\r\n消息：{item.Value.Message}\r\n");
                            break;
                        default:
                            if (item.Value.AddressDataType.ToString().Contains("Array"))
                            {
                                await uiMessage_DataEvent.ShowAsync($"{e.Message}\r\n键：{item.Key}\r\n值：{item.Value.ResultValue.ToJson()}\r\n消息：{item.Value.Message}\r\n");
                            }
                            else
                            {
                                await uiMessage_DataEvent.ShowAsync($"{e.Message}\r\n键：{item.Key}\r\n值：{item.Value.ResultValue}\r\n消息：{item.Value.Message}\r\n");
                            }
                            break;
                    }
                }
            }
            else if (e.GetDetails(out message, out List<ConcurrentDictionary<string, AddressValue>>? datas) && datas is not null)
            {
                foreach (var items in datas)
                {
                    foreach (var item in items)
                    {
                        switch (item.Value.AddressDataType)
                        {
                            case Model.@enum.DataType.ByteArray when item.Value.ResultValue is byte[] bytes:
                                await uiMessage_DataEvent.ShowAsync($"{e.Message}\r\n键：{item.Key}\r\n值：{ByteHandler.ByteToHexString(bytes)}\r\n消息：{item.Value.Message}\r\n");
                                break;
                            default:
                                if (item.Value.AddressDataType.ToString().Contains("Array"))
                                {
                                    await uiMessage_DataEvent.ShowAsync($"{e.Message}\r\n键：{item.Key}\r\n值：{item.Value.ResultValue.ToJson()}\r\n消息：{item.Value.Message}\r\n");
                                }
                                else
                                {
                                    await uiMessage_DataEvent.ShowAsync($"{e.Message}\r\n键：{item.Key}\r\n值：{item.Value.ResultValue}\r\n消息：{item.Value.Message}\r\n");
                                }
                                break;
                        }
                    }
                }
            }
        }

        private async Task Daq_OnInfoEventAsync(object? sender, EventInfoResult e)
        {
            await uiMessage_DataEvent.ShowAsync(e.ToJson(true));
        }



        /// <summary>
        /// 打开
        /// </summary>
        public IAsyncRelayCommand On => p_On ??= new AsyncRelayCommand(OnAsync);
        IAsyncRelayCommand? p_On;
        public async Task OnAsync()
        {
            await initializationTask;
            if (Communication == null)
            {
                Communication = new OpcUaServiceOperate(BasicsData);
            }

            OpcUaServiceOperate instance = Communication;
            instance.OnInfoEventAsync -= Daq_OnInfoEventAsync;
            instance.OnInfoEventAsync += Daq_OnInfoEventAsync;
            instance.OnDataEventAsync -= Daq_OnDataEventAsync;
            instance.OnDataEventAsync += Daq_OnDataEventAsync;
            var result = await instance.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message ?? string.Empty);

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
            if (Communication == null)
            {
                return;  // 未启动，无需关闭
            }
            var result = await Communication.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message ?? string.Empty);
            if (result.Status)
            {
                Communication.OnInfoEventAsync -= Daq_OnInfoEventAsync;
                Communication.OnDataEventAsync -= Daq_OnDataEventAsync;
            }

            DeviceStatusFlashing = (await Communication.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 读取
        /// </summary>
        public IAsyncRelayCommand Read => p_Read ??= new AsyncRelayCommand(ReadAsync);
        IAsyncRelayCommand? p_Read;
        public async Task ReadAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            await uiMessage_InfoEvent.ShowAsync((await instance.ReadAsync(OrganizationAddress())).ResultData.ToJson(true));

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 写入
        /// </summary>
        public IAsyncRelayCommand Write => p_Write ??= new AsyncRelayCommand(WriteAsync);
        IAsyncRelayCommand? p_Write;
        public async Task WriteAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            ConcurrentDictionary<string, WriteModel> pairs = new ConcurrentDictionary<string, WriteModel>();
            Model.@enum.DataType dataType = (Model.@enum.DataType)DataType + 1;
            pairs.TryAdd(DotAddress, new WriteModel(WriteInData, dataType));
            await uiMessage_InfoEvent.ShowAsync((await instance.WriteAsync(pairs)).Message ?? string.Empty);

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 创建
        /// </summary>
        public IAsyncRelayCommand Add => p_Add ??= new AsyncRelayCommand(AddAsync);
        IAsyncRelayCommand? p_Add;
        public async Task AddAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;

            Model.@enum.DataType dataType = (Model.@enum.DataType)DataType + 1;
            BuiltInType type = new BuiltInType();
            object? value = null;

            try
            {
                switch (dataType)
                {
                    case Model.@enum.DataType.Bool:
                        type = BuiltInType.Boolean;
                        value = bool.Parse(WriteInData);
                        break;
                    case Model.@enum.DataType.Double:
                        type = BuiltInType.Double;
                        value = double.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Float:
                    case Model.@enum.DataType.Single:
                        type = BuiltInType.Float;
                        value = float.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Short:
                    case Model.@enum.DataType.Int16:
                        type = BuiltInType.Int16;
                        value = Int16.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Ushort:
                    case Model.@enum.DataType.UInt16:
                        type = BuiltInType.UInt16;
                        value = UInt16.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Int:
                    case Model.@enum.DataType.Int32:
                        type = BuiltInType.Int32;
                        value = Int32.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Uint:
                    case Model.@enum.DataType.UInt32:
                        type = BuiltInType.UInt32;
                        value = UInt32.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Long:
                    case Model.@enum.DataType.Int64:
                        type = BuiltInType.Int64;
                        value = Int64.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.Ulong:
                    case Model.@enum.DataType.UInt64:
                        type = BuiltInType.UInt64;
                        value = UInt64.Parse(WriteInData, CultureInfo.InvariantCulture);
                        break;
                    case Model.@enum.DataType.String:
                    case Model.@enum.DataType.Char:
                        type = BuiltInType.String;
                        value = WriteInData ?? string.Empty;
                        break;
                }
            }
            catch (Exception ex)
            {
                await uiMessage_InfoEvent.ShowAsync($"{App.LanguageOperate.GetLanguageValue("数据格式不正确")}：{ex.Message}");
                return;
            }
            if (value == null)
            {
                await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("默认值不能为空") ?? "默认值不能为空");
                return;
            }
            OperateResult operateResult = instance.CreateAddress(new List<Opc.core.AddressBody> { new Opc.core.AddressBody
                            {
                                AddressName=DotAddress,
                                Dynamic=false,
                                DefaultValue=value,
                                DataType=type,
                                AccessLevel=3,
                            } }, ComboBoxSelectedItem?.Value as FolderState);
            await uiMessage_InfoEvent.ShowAsync(operateResult.Message ?? string.Empty);
        }

        /// <summary>
        /// 移除
        /// </summary>
        public IAsyncRelayCommand Remove => p_Remove ??= new AsyncRelayCommand(RemoveAsync);
        IAsyncRelayCommand? p_Remove;
        public async Task RemoveAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            OperateResult operateResult = instance.RemoveAddress(new List<Opc.core.AddressBody> { new Opc.core.AddressBody { AddressName = DotAddress, Dynamic = false } });
            await uiMessage_InfoEvent.ShowAsync(operateResult.Message ?? string.Empty);

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        public IAsyncRelayCommand CreateFolder => p_CreateFolder ??= new AsyncRelayCommand(CreateFolderAsync);
        IAsyncRelayCommand? p_CreateFolder;
        public async Task CreateFolderAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            if (FolderName.IsNullOrEmpty())
            {
                await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("父级名称不能为空") ?? "父级名称不能为空");
            }
            else
            {
                OperateResult operateResult = instance.CreateFolder(FolderName, ComboBoxSelectedItem?.Value as FolderState);
                if (operateResult.Status)
                {
                    Application? application = Application.Current;
                    if (application == null || operateResult.ResultData is not FolderState folder)
                        return;
                    await application.Dispatcher.InvokeAsync(delegate ()
                    {
                        // 首次创建（无选中父级）时 ComboBoxSelectedItem 为 null，键直接取 FolderName
                        string folderKey = ComboBoxSelectedItem is null
                            ? FolderName
                            : $"{ComboBoxSelectedItem.Key}.{FolderName}";
                        ComboBoxModel comboBox = new ComboBoxModel(folderKey, folder);
                        ComboBoxItemsSource.Add(comboBox);
                        ComboBoxSelectedItem = comboBox;
                    });

                    await uiMessage_InfoEvent.ShowAsync($"[ {FolderName} ] {App.LanguageOperate.GetLanguageValue("父级创建成功") ?? "父级创建成功"}");
                }
                else
                {
                    await uiMessage_InfoEvent.ShowAsync($"[ {FolderName} ] {App.LanguageOperate.GetLanguageValue("父级创建失败") ?? "父级创建失败"}，{operateResult.Message ?? string.Empty}");
                }
            }

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 导入节点
        /// </summary>
        public IAsyncRelayCommand IncDot => p_IncDot ??= new AsyncRelayCommand(IncDotAsync);
        IAsyncRelayCommand? p_IncDot;
        public async Task IncDotAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            string file = SelectFiles("json");
            if (!string.IsNullOrEmpty(file))
            {
                const long maxImportBytes = 10 * 1024 * 1024;
                if (new FileInfo(file).Length > maxImportBytes)
                {
                    await uiMessage_InfoEvent.ShowAsync("导入文件不能超过 10 MB。");
                    return;
                }
                NodeBody? structuralBody = (await File.ReadAllTextAsync(file)).ToJsonEntity<NodeBody>();
                if (structuralBody == null)
                {
                    await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("导入失败") ?? "导入失败");
                }
                else
                {
                    OperateResult operateResult = instance.IncAddress(structuralBody, ComboBoxSelectedItem?.Value as FolderState);
                    if (operateResult.Status)
                    {
                        await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("导入成功") ?? "导入成功");
                    }
                    else
                    {
                        await uiMessage_InfoEvent.ShowAsync($"{App.LanguageOperate.GetLanguageValue("导入失败") ?? "导入失败"}，{operateResult.Message ?? string.Empty}");
                    }
                }
            }

            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }


        /// <summary>
        /// 获取地址集合
        /// </summary>
        public IAsyncRelayCommand GetAddressArray => p_GetAddressArray ??= new AsyncRelayCommand(GetAddressArrayAsync);
        IAsyncRelayCommand? p_GetAddressArray;
        public async Task GetAddressArrayAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            OperateResult operateResult = instance.GetAddressArray();
            await uiMessage_InfoEvent.ShowAsync(operateResult.Message ?? string.Empty);
            await uiMessage_InfoEvent.ShowAsync(operateResult.ResultData.ToJson(true));
            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        /// <summary>
        /// 移除文件夹
        /// </summary>
        public IAsyncRelayCommand RemoveFolder => p_RemoveFolder ??= new AsyncRelayCommand(RemoveFolderAsync);
        IAsyncRelayCommand? p_RemoveFolder;
        public async Task RemoveFolderAsync()
        {
            OpcUaServiceOperate instance = GetCommunication();
            DeviceStatusFlashing = (await instance.GetStatusAsync()).Status;
            TabSelectedIndex = 1;

            if (ComboBoxSelectedItem?.Value is not FolderState selectedFolder)
            {
                await uiMessage_InfoEvent.ShowAsync(App.LanguageOperate.GetLanguageValue("不允许移除") ?? "不允许移除");
                return;
            }
            OperateResult operateResult = instance.RemoveFolder(new List<NodeId> { selectedFolder.NodeId });
            if (operateResult.Status)
            {
                string selectedKey = ComboBoxSelectedItem.Key;
                List<ComboBoxModel> cbs = ComboBoxItemsSource
                    .Where(c => c.Key.Equals(selectedKey, StringComparison.Ordinal) ||
                                c.Key.StartsWith(selectedKey + ".", StringComparison.Ordinal))
                    .ToList();
                foreach (var item in cbs)
                {
                    ComboBoxItemsSource.Remove(item);
                }
                // 移除最后一个/全部文件夹后集合可能为空，避免索引越界
                ComboBoxSelectedItem = ComboBoxItemsSource.Count > 0 ? ComboBoxItemsSource[^1] : null;
                await uiMessage_InfoEvent.ShowAsync($"[ {FolderName} ] {App.LanguageOperate.GetLanguageValue("父级移除成功") ?? "父级移除成功"}");
            }
            else
            {
                await uiMessage_InfoEvent.ShowAsync($"[ {FolderName} ] {App.LanguageOperate.GetLanguageValue("父级移除失败") ?? "父级移除失败"}，{operateResult.Message ?? string.Empty}");
            }
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


        /// <summary>
        /// 组织地址
        /// </summary>
        /// <returns></returns>
        private Address OrganizationAddress()
        {
            Model.@enum.DataType dataType = (Model.@enum.DataType)DataType + 1;
            Address address = new Address();
            address.AddressArray = new List<AddressDetails>()
            {
                new AddressDetails()
                {
                    AddressName = DotAddress,
                    AddressDataType=dataType,
                    EncodingType=Model.@enum.EncodingType.ANSI,
                }
            };
            return address;
        }

        /// <summary>
        /// 选中文件
        /// </summary>
        /// <param name="fileExt">文件格式</param>
        /// <returns></returns>
        public string SelectFiles(string fileExt)
        {
            var filters = new Dictionary<string, string>
            {
                { $"(*.{fileExt})", $"*.{fileExt}" },
            };
            return Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件") ?? "请选择文件", false, filters);
        }


        public static string SelectFolder()
        {
            return Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件夹") ?? "请选择文件夹", true);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            Communication?.Dispose();
            uiMessage_DataEvent.Dispose();
            uiMessage_InfoEvent.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            await initializationTask;
            if (Communication != null) await Communication.DisposeAsync();
            await uiMessage_DataEvent.DisposeAsync();
            await uiMessage_InfoEvent.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        /// <summary>取得已启动的 OPC UA 服务实例。</summary>
        private OpcUaServiceOperate GetCommunication()
        {
            return Communication ?? throw new InvalidOperationException("OPC UA service has not been started.");
        }
    }
}
