using Microsoft.Extensions.DependencyInjection;
using Snet.Core.handler;
using Snet.Iot.Debug.view;
using Snet.Log;
using Snet.Model.data;
using Snet.Windows.Controls.data;
using Snet.Windows.Controls.property;
using Snet.Windows.Core.handler;
using System.Windows;
namespace Snet.Iot.Debug
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 语言操作
        /// </summary>
        public readonly static LanguageModel LanguageOperate = new LanguageModel("Snet.Iot.Debug", "Language", "Snet.Iot.Debug.dll");
        /// <summary>
        /// 图片资源路径
        /// </summary>
        public readonly static string IconResourcePath = "pack://application:,,,/Snet.Iot.Debug;component/resources/icons.xaml";

        /// <summary>
        /// 设备加载
        /// </summary>
        public static TabDeviceControlModel? tabDeviceModel = null;

        /// <summary>
        /// 设备对象类型
        /// </summary>
        public readonly static Type tabDeviceType = typeof(TabDeviceControl);

        /// <summary>
        /// 弹窗标识符
        /// </summary>
        public static readonly string dialogHostTag = "DialogHost";

        /// <summary>
        /// 缓存参数对话框
        /// </summary>
        public static readonly PropertyControl param = InjectionWpf.GetService<PropertyControl>();

        /// <summary>
        /// 信息框模型集合
        /// </summary>
        public readonly static List<EditModel> EditModels = GetEditModels();

        /// <summary>
        /// 获取信息框模型集合，定义日志输出文本的颜色高亮规则
        /// </summary>
        /// <returns>编辑模型集合，包含各类日志标签对应的高亮颜色</returns>
        private static List<EditModel> GetEditModels() =>
        [
            new() { Name = "[ Info ]",                Color = "#4CAF50" },
            new() { Name = "[ Error ]",               Color = "#F44336" },
            new() { Name = "异常",                    Color = "#F44336" },
            new() { Name = "Exception",               Color = "#F44336" },
            new() { Name = "[ Mq ]",                  Color = "#2196F3" },
            new() { Name = "[ Daq ]",                 Color = "#2196F3" },
            new() { Name = "[ MqttService ]",         Color = "#2196F3" },
            new() { Name = "[ OpcUaService ]",        Color = "#2196F3" },
            new() { Name = "[ MqttServiceOperate ]",  Color = "#2196F3" },
            new() { Name = "[ OpcUaServiceOperate ]", Color = "#2196F3" },
            new() { Name = "TRUE",                    Color = "#4CAF50" },
            new() { Name = "FALSE",                   Color = "#F44336" },
            new() { Name = ">",                       Color = "#FBC31D" },
            new() { Name = "<",                       Color = "#FBC31D" },
            new() { Name = "On",                            Color = "#00BCD4" },
            new() { Name = "OnAsync",                       Color = "#00BCD4" },
            new() { Name = "Off",                           Color = "#00BCD4" },
            new() { Name = "OffAsync",                      Color = "#00BCD4" },
            new() { Name = "Read",                          Color = "#00BCD4" },
            new() { Name = "ReadAsync",                     Color = "#00BCD4" },
            new() { Name = "Write",                         Color = "#00BCD4" },
            new() { Name = "WriteAsync",                    Color = "#00BCD4" },
            new() { Name = "Send",                          Color = "#00BCD4" },
            new() { Name = "SendAsync",                     Color = "#00BCD4" },
            new() { Name = "SendWait",                      Color = "#00BCD4" },
            new() { Name = "SendWaitAsync",                 Color = "#00BCD4" },
            new() { Name = "Subscribe",                     Color = "#00BCD4" },
            new() { Name = "SubscribeAsync",                Color = "#00BCD4" },
            new() { Name = "UnSubscribe",                   Color = "#00BCD4" },
            new() { Name = "UnSubscribeAsync",              Color = "#00BCD4" },
            new() { Name = "Produce",                       Color = "#00BCD4" },
            new() { Name = "ProduceAsync",                  Color = "#00BCD4" },
            new() { Name = "Consume",                       Color = "#00BCD4" },
            new() { Name = "ConsumeAsync",                  Color = "#00BCD4" },
            new() { Name = "UnConsume",                     Color = "#00BCD4" },
            new() { Name = "UnConsumeAsync",                Color = "#00BCD4" },
            new() { Name = "LogOperateSet",                 Color = "#00BCD4" },
            new() { Name = "LogOperateSetAsync",            Color = "#00BCD4" },
            new() { Name = "LogOperateGet",                 Color = "#00BCD4" },
            new() { Name = "LogOperateGetAsync",            Color = "#00BCD4" },
            new() { Name = "GetNodeID",                     Color = "#00BCD4" },
            new() { Name = "GetAllNode",                    Color = "#00BCD4" },
            new() { Name = "GetNodeIconType",               Color = "#00BCD4" },
            new() { Name = "DetailedReadAllNodeData",       Color = "#00BCD4" },
            new() { Name = "GetAccessLevel",                Color = "#00BCD4" },
            new() { Name = "TypeConvert",                   Color = "#00BCD4" },
            new() { Name = "GetNodeValueType",              Color = "#00BCD4" },
            new() { Name = "WAOn",                          Color = "#00BCD4" },
            new() { Name = "WAOnAsync",                     Color = "#00BCD4" },
            new() { Name = "WAOff",                         Color = "#00BCD4" },
            new() { Name = "WAOffAsync",                    Color = "#00BCD4" },
            new() { Name = "WAStatus",                      Color = "#00BCD4" },
            new() { Name = "WAStatusAsync",                 Color = "#00BCD4" },
            new() { Name = "WARequestExample",              Color = "#00BCD4" },
            new() { Name = "WARequestExampleAsync",         Color = "#00BCD4" },
            new() { Name = "GetLanguageValue",              Color = "#00BCD4" },
            new() { Name = "GetLanguageValueAsync",         Color = "#00BCD4" },
            new() { Name = "GetLanguage",                   Color = "#00BCD4" },
            new() { Name = "GetLanguageAsync",              Color = "#00BCD4" },
            new() { Name = "SetLanguage",                   Color = "#00BCD4" },
            new() { Name = "SetLanguageAsync",              Color = "#00BCD4" },
            new() { Name = "Remove",                        Color = "#00BCD4" },
            new() { Name = "RemoveAsync",                   Color = "#00BCD4" },
            new() { Name = "GetStatus",                     Color = "#00BCD4" },
            new() { Name = "GetStatusAsync",                Color = "#00BCD4" },
            new() { Name = "CreateInstance",                Color = "#00BCD4" },
            new() { Name = "CreateInstanceAsync",           Color = "#00BCD4" },
            new() { Name = "GetBaseObject",                 Color = "#00BCD4" },
            new() { Name = "GetBaseObjectAsync",            Color = "#00BCD4" },
            new() { Name = "CloneThis",                     Color = "#00BCD4" },
            new() { Name = "CloneThisAsync",                Color = "#00BCD4" },
            new() { Name = "UpdateArgs",                    Color = "#00BCD4" },
            new() { Name = "UpdateArgsAsync",               Color = "#00BCD4" },
            new() { Name = "GetBasicsArgs",                 Color = "#00BCD4" },
            new() { Name = "GetBasicsArgsAsync",            Color = "#00BCD4" },
            new() { Name = "GetArgs",                       Color = "#00BCD4" },
            new() { Name = "GetArgsAsync",                  Color = "#00BCD4" },
            new() { Name = "GetAutoAllocatingArgs",         Color = "#00BCD4" },
            new() { Name = "GetAutoAllocatingArgsAsync",    Color = "#00BCD4" },
            new() { Name = "ExistsAutoAllocatingArgs",      Color = "#00BCD4" },
            new() { Name = "ExistsAutoAllocatingArgsAsync", Color = "#00BCD4" },
            new() { Name = "Packer",                        Color = "#00BCD4" },
            new() { Name = "PackerAsync",                   Color = "#00BCD4" },
            new() { Name = "UnPacker",                      Color = "#00BCD4" },
            new() { Name = "UnPackerAsync",                 Color = "#00BCD4" },
            new() { Name = "SUCCESS",                           Color = "#4CAF50" },
            new() { Name = "[OK]",                              Color = "#4CAF50" },
            new() { Name = "Produce success",                   Color = "#4CAF50" },
            new() { Name = "FAILURE",                           Color = "#F44336" },
            new() { Name = "[ZIP FAIL]",                        Color = "#F44336" },
            new() { Name = "解包失败",                              Color = "#F44336" },
            new() { Name = "插件尚未加载",                            Color = "#F44336" },
            new() { Name = "地址自动组包失败",                          Color = "#F44336" },
            new() { Name = "下载失败",                              Color = "#F44336" },
            new() { Name = "Download failed",                   Color = "#F44336" },
            new() { Name = "删除目录失败",                            Color = "#F44336" },
            new() { Name = "[ZIP]",                             Color = "#00BCD4" },
            new() { Name = "[SingleInstance]",                  Color = "#FF9800" },
            new() { Name = "exceed limit value",                Color = "#FF9800" },
            new() { Name = "下载已被取消",                            Color = "#FF9800" },
            new() { Name = "Download canceled",                 Color = "#FF9800" },
            new() { Name = "[ UaSyncChannelDataEventAsync ]",   Color = "#9C27B0" },
            new() { Name = "[ DataSyncChannelDataEventAsync ]", Color = "#9C27B0" },
            new() { Name = "set event",                         Color = "#607D8B" },
            new() { Name = "Snet.Mqtt",                         Color = "#3F51B5" },
            new() { Name = "Snet.Kafka",                        Color = "#3F51B5" },
            new() { Name = "Snet.RabbitMQ",                     Color = "#3F51B5" },
            new() { Name = "Snet.Netty",                        Color = "#3F51B5" },
            new() { Name = "Snet.NetMQ",                        Color = "#3F51B5" },
            new() { Name = "Snet.AllenBradley",                 Color = "#3F51B5" },
            new() { Name = "Snet.Beckhoff",                     Color = "#3F51B5" },
            new() { Name = "Snet.DB",                           Color = "#3F51B5" },
            new() { Name = "Snet.Delta",                        Color = "#3F51B5" },
            new() { Name = "Snet.Fatek",                        Color = "#3F51B5" },
            new() { Name = "Snet.Fuji",                         Color = "#3F51B5" },
            new() { Name = "Snet.GE",                           Color = "#3F51B5" },
            new() { Name = "Snet.Inovance",                     Color = "#3F51B5" },
            new() { Name = "Snet.Invt",                         Color = "#3F51B5" },
            new() { Name = "Snet.Keyence",                      Color = "#3F51B5" },
            new() { Name = "Snet.LSis",                         Color = "#3F51B5" },
            new() { Name = "Snet.MegMeet",                      Color = "#3F51B5" },
            new() { Name = "Snet.Mitsubishi",                   Color = "#3F51B5" },
            new() { Name = "Snet.Modbus",                       Color = "#3F51B5" },
            new() { Name = "Snet.Opc",                          Color = "#3F51B5" },
            new() { Name = "Snet.Panasonic",                    Color = "#3F51B5" },
            new() { Name = "Snet.PQDIF",                        Color = "#3F51B5" },
            new() { Name = "Snet.Siemens",                      Color = "#3F51B5" },
            new() { Name = "Snet.Omron",                        Color = "#3F51B5" },
            new() { Name = "Snet.Sim",                          Color = "#3F51B5" },
            new() { Name = "Snet.TEP",                          Color = "#3F51B5" },
            new() { Name = "Snet.Toyota",                       Color = "#3F51B5" },
            new() { Name = "Snet.Vigor",                        Color = "#3F51B5" },
            new() { Name = "Snet.WeCon",                        Color = "#3F51B5" },
            new() { Name = "Snet.XinJE",                        Color = "#3F51B5" },
            new() { Name = "Snet.Yamatake",                     Color = "#3F51B5" },
            new() { Name = "Snet.Yaskawa",                      Color = "#3F51B5" },
            new() { Name = "Snet.Yokogawa",                     Color = "#3F51B5" },
            new() { Name = "Snet.Freedom",                      Color = "#3F51B5" },
            new() { Name = "Snet.RKC",                          Color = "#3F51B5" },
            new() { Name = "Snet.Turck",                        Color = "#3F51B5" },
            new() { Name = "Snet.Fanuc",                        Color = "#3F51B5" },
            new() { Name = "Snet.OrientalMotor",                Color = "#3F51B5" },
            new() { Name = "Snet.Kossi",                        Color = "#3F51B5" },
            new() { Name = "Snet.YuDian",                       Color = "#3F51B5" },
            new() { Name = "服务启动事件",                 Color = "#00BCD4" },
            new() { Name = "服务停止事件",                 Color = "#00BCD4" },
            new() { Name = "客户端订阅事件",                Color = "#00BCD4" },
            new() { Name = "客户端取消订阅事件",              Color = "#00BCD4" },
            new() { Name = "客户端消息事件",                Color = "#00BCD4" },
            new() { Name = "客户端连接事件",                Color = "#00BCD4" },
            new() { Name = "客户端断开事件",                Color = "#00BCD4" },
            new() { Name = "客户端身份验证事件",              Color = "#00BCD4" },
            new() { Name = "系统信息",                   Color = "#00BCD4" },
            new() { Name = "系统异常信息",                 Color = "#F44336" },
            new() { Name = "连接成功",                   Color = "#4CAF50" },
            new() { Name = "异常断开",                   Color = "#F44336" },
            new() { Name = "重连中",                    Color = "#FF9800" },
            new() { Name = "重连成功",                   Color = "#4CAF50" },
            new() { Name = "订阅通知",                   Color = "#4CAF50" },
            new() { Name = "事件对象不匹配",                Color = "#FF9800" },
            new() { Name = "客户端连接",                  Color = "#4CAF50" },
            new() { Name = "客户端断开",                  Color = "#FF9800" },
            new() { Name = "消息接收",                   Color = "#4CAF50" },
            new() { Name = "->",                     Color = "#4CAF50" },
            new() { Name = "点位数据更新",                 Color = "#4CAF50" },
            new() { Name = "消费异常",                   Color = "#F44336" },
            new() { Name = "MQTT 服务端口被占用",           Color = "#F44336" },
            new() { Name = "MQTT WebSocket 服务端口被占用", Color = "#F44336" },
        ];

        /// <summary>
        /// 在应用程序关闭时发生
        /// </summary>
        private void OnExit(object sender, ExitEventArgs e)
        {
            InjectionWpf.ClearService();
            GC.SuppressFinalize(this);
            GC.Collect();
        }

        /// <summary>
        /// 在加载应用程序时发生
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // 注入参数设置控件
            PropertyControl control = new PropertyControl();
            control.ButtonVisibility = Visibility.Visible;
            InjectionWpf.AddService(s =>
            {
                s.AddSingleton(control);
                s.AddSingleton<About>();

                s.AddTransient<Daq>();
                s.AddTransient<OpcUaService>();
                s.AddTransient<Mq>();
                s.AddTransient<MqttService>();
                s.AddTransient<MqttWebSocketService>();
                s.AddTransient<NettyService>();
                s.AddTransient<Communication>();
                s.AddTransient<CommunicationService>();
                s.AddTransient<Svg>();
                s.AddTransient<Gif>();
                s.AddTransient<OpcUaNodeBrowsing>();
            });


            //启动全局异常捕捉
            RegisterEvents();
            //加载本地自定义图标
            IconsHandler.Loading(IconResourcePath);
            //打开主窗口
            InjectionWpf.Window<MainWindow, MainWindowModel>(true).Show();
        }

        #region 全局异常捕捉

        /// <summary>
        /// 注册全局异常捕获事件，覆盖 Task 线程、UI 线程和非 UI 线程的未处理异常
        /// </summary>
        private void RegisterEvents()
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //UI线程未捕获异常处理事件（UI主线程）
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Task 线程内未捕获异常处理事件，先进行空判断再访问 HResult 属性以避免空引用异常。
        /// </summary>
        private async void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var exception = e.Exception as Exception;
                if (exception == null)
                    return;

                // HResult = -2146233088 (0x80131500) 对应 ExternalException 基类，
                // 通常为正常取消或可忽略的系统级异常，不弹窗处理。
                if (exception.HResult == -2146233088)
                    return;

                await HandleException(exception);
            }
            catch (Exception ex)
            {
                await HandleException(ex);
            }
            finally
            {
                e.SetObserved();
            }
        }

        /// <summary>
        /// 非UI线程未捕获异常处理事件（例如自己创建的子线程）
        /// </summary>
        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    await HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                await HandleException(ex);
            }
        }

        /// <summary>
        /// UI线程未捕获异常处理事件（UI主线程）
        /// </summary>
        private async void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 先标记已处理，避免 WPF 在 await 后重复触发
            e.Handled = true;
            try
            {
                await HandleException(e.Exception);
            }
            catch (Exception ex)
            {
                await HandleException(ex);
            }
        }

        /// <summary>
        /// 处理异常到界面显示与本地日志记录
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleException(Exception e)
        {
            string source = e.Source ?? string.Empty;
            string message = e.Message ?? string.Empty;
            string stackTrace = e.StackTrace ?? string.Empty;
            string msg;
            if (!string.IsNullOrEmpty(source))
            {
                msg = source;
                if (!string.IsNullOrEmpty(message))
                    msg += $"\r\n{message}";
                if (!string.IsNullOrEmpty(stackTrace))
                    msg += $"\r\n\r\n{stackTrace}";
            }
            else if (!string.IsNullOrEmpty(message))
            {
                msg = message;
                if (!string.IsNullOrEmpty(stackTrace))
                    msg += $"\r\n\r\n{stackTrace}";
            }
            else if (!string.IsNullOrEmpty(stackTrace))
                msg = stackTrace;
            else
                msg = "未知异常";
            if (Application.Current == null)
                return;
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await Snet.Windows.Controls.message.MessageBox.Show(msg, LanguageOperate.GetLanguageValue("全局异常捕获"), Snet.Windows.Controls.@enum.MessageBoxButton.OK, Snet.Windows.Controls.@enum.MessageBoxImage.Exclamation);
            }
            , System.Windows.Threading.DispatcherPriority.Loaded);

            LogHelper.Error(msg, "Snet.Iot.Debug", e);
        }

        #endregion

    }

}
