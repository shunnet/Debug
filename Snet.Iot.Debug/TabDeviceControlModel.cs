using CommunityToolkit.Mvvm.Input;
using Snet.Iot.Debug.model;
using Snet.Windows.Core.handler;
using Snet.Windows.Core.mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Application = System.Windows.Application;

namespace Snet.Iot.Debug
{
    public class TabDeviceControlModel : BindNotify
    {
        public TabDeviceControlModel()
        {
            AutoCheckTabControlVisibility();
        }

        /// <summary>
        /// 设备点位
        /// </summary>
        public ObservableCollection<TabControlDeviceModel> Devices
        {
            get => devices;
            set => SetProperty(ref devices, value);
        }
        private ObservableCollection<TabControlDeviceModel> devices = new ObservableCollection<TabControlDeviceModel>();

        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="nameKey">名称的Key</param>
        /// <param name="control">用户控件</param>
        public void AddDevice(string nameKey, System.Windows.Controls.UserControl control)
        {
            SelectedDevicesItem = new TabControlDeviceModel(nameKey, control);
            Devices.Insert(0, SelectedDevicesItem);
            AutoCheckTabControlVisibility();
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SkinHandler.SetSkin(SkinHandler.GetSkin());
            }, System.Windows.Threading.DispatcherPriority.Background);

        }

        /// <summary>
        /// 设置已存在的项选中,移动到最前方
        /// </summary>
        /// <param name="nameKey">名称的Key</param>
        /// <returns>成功或失败</returns>
        public void SetSelected(string nameKey, System.Windows.Controls.UserControl control)
        {
            TabControlDeviceModel? device = devices.FirstOrDefault(s => s.NameKey == nameKey);
            if (device == null)
            {
                AddDevice(nameKey, control);
                return;
            }
            // 先移除
            Devices.Remove(device);
            // 再插入到最前面
            Devices.Insert(0, device);
            SelectedDevicesItem = device;
        }

        /// <summary>
        /// 选中的设备项
        /// </summary>
        public TabControlDeviceModel SelectedDevicesItem
        {
            get => GetProperty(() => SelectedDevicesItem);
            set => SetProperty(() => SelectedDevicesItem, value);
        }

        /// <summary>
        /// 显示隐藏
        /// </summary>
        public Visibility TabControlVisibility
        {
            get => GetProperty(() => TabControlVisibility);
            set => SetProperty(() => TabControlVisibility, value);
        }

        /// <summary>
        /// 自动检查tab控件是否显示隐藏
        /// </summary>
        public void AutoCheckTabControlVisibility()
        {
            if (Devices.Count > 0)
            {
                TabControlVisibility = Visibility.Visible;
            }
            else
            {
                TabControlVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 关闭 tab 项
        /// </summary>
        public IAsyncRelayCommand CloseTabCommand => closeTabCommand ??= new AsyncRelayCommand<TabControlDeviceModel>(CloseTabCommandAsync);
        private IAsyncRelayCommand? closeTabCommand;
        private async Task CloseTabCommandAsync(TabControlDeviceModel? tab)
        {
            if (tab != null)
            {
                Devices.Remove(tab);
                await tab.DisposeAsync();
                AutoCheckTabControlVisibility();
            }
        }

        /// <summary>
        /// 关闭其他 tab 项
        /// </summary>
        public IAsyncRelayCommand CloseOthersTabCommand => closeOthersTabCommand ??= new AsyncRelayCommand<TabControlDeviceModel>(CloseOthersTabCommandAsync);
        private IAsyncRelayCommand? closeOthersTabCommand;
        private async Task CloseOthersTabCommandAsync(TabControlDeviceModel? tab)
        {
            if (tab == null) return;

            // 从后向前遍历，安全删除
            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                var item = Devices[i];
                if (item != tab)
                {
                    await item.DisposeAsync();   // 释放资源
                    Devices.RemoveAt(i);         // 从集合中移除
                }
            }
        }

        /// <summary>
        /// 关闭全部 tab 项
        /// </summary>
        public IAsyncRelayCommand CloseAllTabCommand => closeAllTabCommand ??= new AsyncRelayCommand(CloseAllTabCommandAsync);
        private IAsyncRelayCommand? closeAllTabCommand;
        private async Task CloseAllTabCommandAsync()
        {
            foreach (var item in Devices)
            {
                await item.DisposeAsync();
            }
            Devices.Clear();
            AutoCheckTabControlVisibility();
        }


        /// <summary>
        /// 鼠标右键点击触发
        /// </summary>
        public IAsyncRelayCommand TabControl_PreviewMouseRightButtonDown => tabControl_PreviewMouseRightButtonDown ??= new AsyncRelayCommand<TabControlDeviceModel>(TabControl_PreviewMouseRightButtonDownAsync);
        private IAsyncRelayCommand? tabControl_PreviewMouseRightButtonDown;
        private Task TabControl_PreviewMouseRightButtonDownAsync(TabControlDeviceModel? device)
        {
            if (device == null)
                return Task.CompletedTask;

            // 选中当前 Tab
            SelectedDevicesItem = device;

            return Task.CompletedTask;
        }


    }
}
