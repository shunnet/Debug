using CommunityToolkit.Mvvm.Input;
using Snet.Core.handler;
using Snet.Iot.Debug.handler;
using Snet.Utility;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;
using System.Windows;
using System.Windows.Controls;
using MessageBox = Snet.Windows.Controls.message.MessageBox;

namespace Snet.Iot.Debug.viewModel
{
    public class GifModel : BindNotify
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FliePath
        {
            get => GetProperty(() => FliePath);
            set => SetProperty(() => FliePath, value);
        }

        /// <summary>
        /// FFmpeg 工具路径
        /// </summary>
        public string FFmpegTool
        {
            get => ffmpegTool;
            set => SetProperty(ref ffmpegTool, value);
        }
        private string ffmpegTool = System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "ffmpeg", "ffmpeg.exe");

        /// <summary>
        /// 文件存储路径
        /// </summary>
        public string FlieStoragePath
        {
            get => GetProperty(() => FlieStoragePath);
            set => SetProperty(() => FlieStoragePath, value);
        }

        /// <summary>
        /// 输出数据
        /// </summary>
        public string OutData
        {
            get => GetProperty(() => OutData);
            set => SetProperty(() => OutData, value);
        }

        /// <summary>
        /// 信息清空
        /// </summary>
        public IAsyncRelayCommand ResultClear => p_ResultClear ??= new AsyncRelayCommand(ResultClearAsync);
        IAsyncRelayCommand p_ResultClear;
        public Task ResultClearAsync()
        {
            OutData = string.Empty;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 转换
        /// </summary>
        public IAsyncRelayCommand StartConvert => p_StartConvert ??= new AsyncRelayCommand(StartConvertAsync);
        IAsyncRelayCommand p_StartConvert;
        public async Task StartConvertAsync()
        {
            if (!string.IsNullOrEmpty(FlieStoragePath) && !string.IsNullOrEmpty(FliePath))
            {
                using (GifHandler toGifTool = new GifHandler())
                {
                    toGifTool.FFmpegTool = FFmpegTool;
                    Action actionNew = () =>
                    {
                        //响应事件
                        toGifTool.OnResponse = async (msg) =>
                        {
                            await LogShow(msg);
                        };
                        //结束事件
                        toGifTool.OnEnd = async (state) =>
                        {
                            if (Application.Current == null)
                                return;
                            await Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                if (state)
                                {
                                    await MessageBox.Show(App.LanguageOperate.GetLanguageValue("转换成功"), App.LanguageOperate.GetLanguageValue("提示"), Windows.Controls.@enum.MessageBoxButton.OK, Windows.Controls.@enum.MessageBoxImage.Information);
                                }
                                else
                                {
                                    await MessageBox.Show(App.LanguageOperate.GetLanguageValue("转换失败"), App.LanguageOperate.GetLanguageValue("提示"), Windows.Controls.@enum.MessageBoxButton.OK, Windows.Controls.@enum.MessageBoxImage.Exclamation);
                                }
                            });
                        };
                        toGifTool.RunConverter(FliePath, FlieStoragePath + $"\\{DateTime.Now.ToString("yyyyMMddHHmmss")}.gif");
                    };
                    //启动线程处理,如果因为异常完成，抛出异常内容
                    Task convertTask = Task.Factory.StartNew(actionNew);
                    await convertTask.ContinueWith(async t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            await LogShow(t.Exception.InnerException == null ? t.Exception.Message : t.Exception.InnerException.Message);
                        }
                    });
                }
            }
            else
            {
                await MessageBox.Show(App.LanguageOperate.GetLanguageValue("路径不能为空"), App.LanguageOperate.GetLanguageValue("提示"), Windows.Controls.@enum.MessageBoxButton.OK, Windows.Controls.@enum.MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// 信息框事件
        /// </summary>
        public IAsyncRelayCommand OutDataTextChanged => p_OutDataTextChanged ??= new AsyncRelayCommand<TextChangedEventArgs>(OutDataTextChangedAsync);
        IAsyncRelayCommand p_OutDataTextChanged;
        /// <summary>
        /// 信息框事件
        /// 让滚动条一直处在最下方
        /// </summary>
        public Task OutDataTextChangedAsync(TextChangedEventArgs? e)
        {
            TextBox textBox = e.Source.GetSource<TextBox>();
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 日志显示
        /// </summary>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public async Task LogShow(string? msg, bool isDateTime = true)
        {
            if (msg.IsNullOrWhiteSpace())
                return;
            if (Application.Current == null)
                return;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (OutData?.Length > 10000)
                {
                    OutData = string.Empty;
                }
                if (isDateTime)
                {
                    OutData += $" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} : {msg}\r\n";
                }
                else
                {
                    OutData += $"{msg}\r\n";
                }
            });
        }



        /// <summary>
        /// 文件夹
        /// </summary>
        public IAsyncRelayCommand SelectFlieStoragePath => p_SelectFlieStoragePath ??= new AsyncRelayCommand(SelectFlieStoragePathAsync);
        IAsyncRelayCommand p_SelectFlieStoragePath;
        public Task SelectFlieStoragePathAsync()
        {
            string str = SelectFolder();
            if (!string.IsNullOrEmpty(str))
            {
                FlieStoragePath = str;
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// 文件
        /// </summary>
        public IAsyncRelayCommand SelectFliePath => p_SelectFliePath ??= new AsyncRelayCommand(SelectFliePathAsync);
        IAsyncRelayCommand p_SelectFliePath;
        public Task SelectFliePathAsync()
        {
            var result = SelectFiles();
            if (!string.IsNullOrWhiteSpace(result))
            {
                FliePath = result;
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// 选中文件
        /// </summary>
        /// <returns></returns>
        public string SelectFiles()
        {
            var filters = new Dictionary<string, string>
            {
                { $"(*.mp4)", $"*.mp4" },
                { $"(*.avi)", $"*.avi" },
                { $"(*.flv)", $"*.flv" },
                { $"(*.mkv)", $"*.mkv" },
                { $"(*.rmvb)", $"*.rmvb" },
            };
            return Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件"), false, filters);
        }


        public static string SelectFolder()
        {
            return Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件夹"), true);
        }

    }
}