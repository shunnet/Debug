using System.Diagnostics;
using System.IO;

namespace Snet.Iot.Debug.handler
{
    /// <summary>
    /// GIF 转换处理器，基于 FFmpeg 将视频文件转换为 GIF 格式。
    /// 使用线程安全的 Lazy 单例模式，实现 IDisposable 以释放资源。
    /// </summary>
    public class GifHandler : IDisposable
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
        public Action<string>? OnResponse { get; set; }

        /// <summary>
        /// 转换结束事件回调，参数为 true 表示成功，false 表示失败。
        /// </summary>
        public Action<bool>? OnEnd { get; set; }

        /// <summary>
        /// FFmpeg 可执行文件的完整路径。
        /// </summary>
        public string FFmpegTool { get; set; } = Path.Combine(AppContext.BaseDirectory, "lib", "ffmpeg", "ffmpeg.exe");

        /// <summary>
        /// 处理 FFmpeg 进程的标准错误输出（FFmpeg 将进度信息输出到 stderr）。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">包含输出数据的事件参数</param>
        private void Output(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                OnResponse?.Invoke(e.Data);
            }
        }

        /// <summary>
        /// 运行 FFmpeg 转换，将指定视频文件转换为 GIF。
        /// 使用 palettegen + paletteuse 滤镜以获得高质量 GIF 输出。
        /// </summary>
        /// <param name="filePath">源视频文件路径</param>
        /// <param name="fileStoragePath">输出 GIF 文件存储路径</param>
        public void RunConverter(string filePath, string fileStoragePath)
        {
            if (string.IsNullOrEmpty(FFmpegTool) || !File.Exists(FFmpegTool))
            {
                OnEnd?.Invoke(false);
                return;
            }

            // ffmpeg 参数：一条命令完成 palettegen + paletteuse
            string arguments = $"-i \"{filePath}\" -filter_complex " +
                   "\"fps=25,split [a][b];[a] palettegen=stats_mode=diff [p];[b][p] paletteuse=dither=bayer\" " +
                   "-y \"" + fileStoragePath + "\"";
            try
            {
                using var p = new Process();
                p.StartInfo.FileName = FFmpegTool;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.ErrorDataReceived += Output;
                p.Start();
                p.BeginErrorReadLine();
                p.WaitForExit();

                OnEnd?.Invoke(true);
            }
            catch
            {
                OnEnd?.Invoke(false);
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

