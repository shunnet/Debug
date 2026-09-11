using CommunityToolkit.Mvvm.Input;
using Snet.Core.handler;
using Snet.Windows.Controls.@enum;
using Snet.Windows.Controls.message;
using Snet.Windows.Core.mvvm;

namespace Snet.Iot.Debug.viewModel
{
    /// <summary>将 SVG 片段转换为 WPF 可用代码并管理界面命令。</summary>
    public sealed class SvgModel : BindNotify
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => GetProperty(() => Name);
            set => SetProperty(() => Name, value);
        }

        /// <summary>
        /// 注释
        /// </summary>
        public string Annotation
        {
            get => GetProperty(() => Annotation);
            set => SetProperty(() => Annotation, value);
        }

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
        private string _color = "{DynamicResource ImageColor}";

        /// <summary>
        /// 输入数据
        /// </summary>
        public string InputData
        {
            get => GetProperty(() => InputData);
            set => SetProperty(() => InputData, value);
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
        public IRelayCommand CodeClear => p_CodeClear ??= new RelayCommand(CodeClearValue);
        private IRelayCommand? p_CodeClear;

        /// <summary>清空输入 SVG。</summary>
        private void CodeClearValue()
        {
            InputData = string.Empty;
        }

        /// <summary>
        /// 信息清空
        /// </summary>
        public IRelayCommand ResultClear => p_ResultClear ??= new RelayCommand(ResultClearValue);
        private IRelayCommand? p_ResultClear;

        /// <summary>清空转换结果。</summary>
        private void ResultClearValue()
        {
            OutData = string.Empty;
        }

        /// <summary>
        /// 转换
        /// </summary>
        public IAsyncRelayCommand Transition => p_Transition ??= new AsyncRelayCommand(TransitionAsync);
        private IAsyncRelayCommand? p_Transition;

        /// <summary>校验输入并执行 SVG 代码转换。</summary>
        public async Task TransitionAsync()
        {
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Annotation) && !string.IsNullOrEmpty(InputData))
            {
                string vsCode = string.Empty;
                if (Snet.Utility.SvgHandler.SvgCodeConverter(Name, Annotation, InputData, out vsCode, Color))
                {
                    OutData = vsCode;
                }
                else
                {
                    await MessageBox.Show(App.LanguageOperate.GetLanguageValue("转换失败") ?? "转换失败", App.LanguageOperate.GetLanguageValue("提示") ?? "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                await MessageBox.Show(App.LanguageOperate.GetLanguageValue("数据不能为空") ?? "数据不能为空", App.LanguageOperate.GetLanguageValue("提示") ?? "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }



        /// <summary>
        /// 复制
        /// </summary>
        public IRelayCommand Copy => p_Copy ??= new RelayCommand(CopyValue);
        private IRelayCommand? p_Copy;

        /// <summary>把转换结果复制到剪贴板。</summary>
        private void CopyValue()
        {
            if (OutData == null) return;
            System.Windows.Clipboard.SetDataObject(OutData);
        }


    }
}
