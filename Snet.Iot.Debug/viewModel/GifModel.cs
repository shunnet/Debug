using CommunityToolkit.Mvvm.Input;
using Snet.Core.handler;
using Snet.Iot.Debug.handler;
using Snet.Windows.Controls.handler;
using Snet.Windows.Core.mvvm;
using System.Windows;
using System.Windows.Controls;
using MessageBox = Snet.Windows.Controls.message.MessageBox;

namespace Snet.Iot.Debug.viewModel
{
    /// <summary>
    /// 驱动视频转 GIF 工具界面，并拥有当前 FFmpeg 转换任务的生命周期。
    /// </summary>
    public sealed class GifModel : BindNotify, IAsyncDisposable
    {
        private CancellationTokenSource? conversionCancellation;
        private Task<bool>? conversionTask;
        private readonly object conversionGate = new();
        private int disposed;

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
        IAsyncRelayCommand? p_ResultClear;
        /// <summary>清空转换输出日志。</summary>
        /// <returns>已完成的任务。</returns>
        public Task ResultClearAsync()
        {
            OutData = string.Empty;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 转换
        /// </summary>
        public IAsyncRelayCommand StartConvert => p_StartConvert ??= new AsyncRelayCommand(StartConvertAsync);
        IAsyncRelayCommand? p_StartConvert;
        /// <summary>验证路径并异步运行 FFmpeg；同一模型一次只拥有一个转换任务。</summary>
        /// <returns>转换和结果提示均完成时结束的任务。</returns>
        public async Task StartConvertAsync()
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);

            if (!string.IsNullOrEmpty(FlieStoragePath) && !string.IsNullOrEmpty(FliePath))
            {
                using GifHandler toGifTool = new();
                toGifTool.FFmpegTool = FFmpegTool;
                toGifTool.OnResponse = msg => LogShow(msg);

                CancellationTokenSource cancellation = new();
                string outputPath = System.IO.Path.Combine(FlieStoragePath, $"{DateTime.Now:yyyyMMddHHmmss}.gif");
                Task<bool> runningTask;
                lock (conversionGate)
                {
                    ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
                    if (conversionTask is not null)
                    {
                        cancellation.Dispose();
                        throw new InvalidOperationException("转换正在进行。");
                    }
                    conversionCancellation = cancellation;
                    try
                    {
                        runningTask = toGifTool.RunConverterAsync(FliePath, outputPath, cancellation.Token);
                    }
                    catch
                    {
                        conversionCancellation = null;
                        cancellation.Dispose();
                        throw;
                    }
                    conversionTask = runningTask;
                }
                try
                {
                    bool succeeded = await runningTask;
                    string messageKey = succeeded ? "转换成功" : "转换失败";
                    var image = succeeded
                        ? Windows.Controls.@enum.MessageBoxImage.Information
                        : Windows.Controls.@enum.MessageBoxImage.Exclamation;
                    await MessageBox.Show(
                        App.LanguageOperate.GetLanguageValue(messageKey) ?? messageKey,
                        App.LanguageOperate.GetLanguageValue("提示") ?? "提示",
                        Windows.Controls.@enum.MessageBoxButton.OK,
                        image);
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                    await LogShow("转换已取消。");
                }
                finally
                {
                    lock (conversionGate)
                    {
                        if (ReferenceEquals(conversionTask, runningTask))
                        {
                            conversionTask = null;
                            conversionCancellation = null;
                        }
                    }
                    cancellation.Dispose();
                }
            }
            else
            {
                await MessageBox.Show(App.LanguageOperate.GetLanguageValue("路径不能为空") ?? "路径不能为空", App.LanguageOperate.GetLanguageValue("提示") ?? "提示", Windows.Controls.@enum.MessageBoxButton.OK, Windows.Controls.@enum.MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// 信息框事件
        /// </summary>
        public IAsyncRelayCommand OutDataTextChanged => p_OutDataTextChanged ??= new AsyncRelayCommand<TextChangedEventArgs>(OutDataTextChangedAsync);
        IAsyncRelayCommand? p_OutDataTextChanged;
        /// <summary>
        /// 信息框事件
        /// 让滚动条一直处在最下方
        /// </summary>
        public Task OutDataTextChangedAsync(TextChangedEventArgs? e)
        {
            if (e?.Source is not TextBox textBox)
            {
                return Task.CompletedTask;
            }
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 日志显示
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="isDateTime">是否在消息前附加当前时间。</param>
        /// <returns></returns>
        public async Task LogShow(string? msg, bool isDateTime = true)
        {
            if (string.IsNullOrWhiteSpace(msg))
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
        IAsyncRelayCommand? p_SelectFlieStoragePath;
        /// <summary>选择 GIF 输出目录。</summary>
        /// <returns>选择器关闭时完成的任务。</returns>
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
        IAsyncRelayCommand? p_SelectFliePath;
        /// <summary>选择待转换的视频文件。</summary>
        /// <returns>选择器关闭时完成的任务。</returns>
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
            return Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件") ?? "请选择文件", false, filters);
        }


        /// <summary>打开文件夹选择器。</summary>
        /// <returns>选中的目录；取消时为空字符串。</returns>
        public static string SelectFolder()
        {
            return Win32Handler.Select(App.LanguageOperate.GetLanguageValue("请选择文件夹") ?? "请选择文件夹", true);
        }

        /// <summary>取消并等待当前 FFmpeg 转换，随后终止该模型的使用。</summary>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            CancellationTokenSource? cancellation;
            Task<bool>? runningTask;
            lock (conversionGate)
            {
                cancellation = conversionCancellation;
                runningTask = conversionTask;
            }
            try
            {
                cancellation?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // 转换刚好完成，由转换任务拥有并释放 CTS。
            }
            if (runningTask is not null)
            {
                try
                {
                    await runningTask;
                }
                catch (OperationCanceledException)
                {
                    // 释放触发的正常取消。
                }
            }
            GC.SuppressFinalize(this);
        }

    }
}
