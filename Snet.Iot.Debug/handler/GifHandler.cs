using System.Diagnostics;
using System.IO;

namespace Snet.Iot.Debug.handler
{
    /// <summary>
    /// GIF 转换处理器，基于 FFmpeg 将视频文件转换为 GIF 格式。
    /// 使用线程安全的 Lazy 单例模式，实现 IDisposable 以释放资源。
    /// </summary>
    public sealed class GifHandler : IDisposable
    {
        /// <summary>
        /// 线程安全的延迟初始化单例实例
        /// </summary>
        private static readonly Lazy<GifHandler> _instance = new(() => new GifHandler(), true);

        /// <summary>
        /// 获取当前对象的单例实例（线程安全）。
        /// </summary>
        /// <returns>GifHandler 单例</returns>
        public static GifHandler Instance() => _instance.Value;

        /// <summary>
        /// 转换过程中每行输出的事件回调（用于显示 FFmpeg 输出信息）。
        /// </summary>
        public Func<string, Task>? OnResponse { get; set; }

        /// <summary>
        /// 转换结束事件回调，参数为 true 表示成功，false 表示失败。
        /// </summary>
        public Action<bool>? OnEnd { get; set; }

        /// <summary>
        /// FFmpeg 可执行文件的完整路径。
        /// </summary>
        public string FFmpegTool { get; set; } = Path.Combine(AppContext.BaseDirectory, "lib", "ffmpeg", "ffmpeg.exe");

        /// <summary>
        /// 运行 FFmpeg 转换，将指定视频文件转换为 GIF。
        /// 使用 palettegen + paletteuse 滤镜以获得高质量 GIF 输出。
        /// </summary>
        /// <param name="filePath">源视频文件路径</param>
        /// <param name="fileStoragePath">输出 GIF 文件存储路径</param>
        /// <param name="cancellationToken">用于取消转换并终止 FFmpeg 进程的令牌。</param>
        /// <returns>FFmpeg 正常退出且输出文件存在时为 <see langword="true"/>。</returns>
        public async Task<bool> RunConverterAsync(
            string filePath,
            string fileStoragePath,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileStoragePath);

            if (string.IsNullOrEmpty(FFmpegTool) || !File.Exists(FFmpegTool))
            {
                OnEnd?.Invoke(false);
                return false;
            }

            if (!File.Exists(filePath))
            {
                OnEnd?.Invoke(false);
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(fileStoragePath))!);
            using var process = new Process();
            process.StartInfo.FileName = FFmpegTool;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.ArgumentList.Add("-i");
            process.StartInfo.ArgumentList.Add(filePath);
            process.StartInfo.ArgumentList.Add("-filter_complex");
            process.StartInfo.ArgumentList.Add("fps=25,split [a][b];[a] palettegen=stats_mode=diff [p];[b][p] paletteuse=dither=bayer");
            process.StartInfo.ArgumentList.Add("-y");
            process.StartInfo.ArgumentList.Add(fileStoragePath);

            try
            {
                if (!process.Start())
                {
                    OnEnd?.Invoke(false);
                    return false;
                }

                while (await process.StandardError.ReadLineAsync(cancellationToken) is { } line)
                {
                    if (OnResponse is not null)
                    {
                        await OnResponse(line);
                    }
                }

                await process.WaitForExitAsync(cancellationToken);
                bool succeeded = process.ExitCode == 0 && File.Exists(fileStoragePath);
                OnEnd?.Invoke(succeeded);
                return succeeded;
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
                OnEnd?.Invoke(false);
                throw;
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
            {
                if (OnResponse is not null)
                {
                    await OnResponse(ex.Message);
                }
                OnEnd?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// 释放资源，清空事件回调引用。
        /// </summary>
        public void Dispose()
        {
            OnResponse = null;
            OnEnd = null;
            GC.SuppressFinalize(this);
        }
    }
}

