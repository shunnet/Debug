using CommunityToolkit.Mvvm.Input;
using Snet.Core.handler;
using Snet.Windows.Controls.@enum;
using Snet.Windows.Controls.message;
using Snet.Windows.Core.mvvm;

namespace Snet.Iot.Debug.viewModel
{
    public class SvgModel : BindNotify
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
        public IAsyncRelayCommand CodeClear => p_CodeClear ??= new AsyncRelayCommand(CodeClearAsync);
        IAsyncRelayCommand p_CodeClear;
        public Task CodeClearAsync()
        {
            InputData = string.Empty;
            return Task.CompletedTask;
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
        public IAsyncRelayCommand Transition => p_Transition ??= new AsyncRelayCommand(TransitionAsync);
        IAsyncRelayCommand p_Transition;
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
                    await MessageBox.Show(App.LanguageOperate.GetLanguageValue("转换失败"), App.LanguageOperate.GetLanguageValue("提示"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                await MessageBox.Show(App.LanguageOperate.GetLanguageValue("数据不能为空"), App.LanguageOperate.GetLanguageValue("提示"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }



        /// <summary>
        /// 复制
        /// </summary>
        public IAsyncRelayCommand Copy => p_Copy ??= new AsyncRelayCommand(CopyAsync);
        IAsyncRelayCommand p_Copy;
        public Task CopyAsync()
        {
            if (OutData == null) return Task.CompletedTask;
            System.Windows.Clipboard.SetDataObject(OutData);
            return Task.CompletedTask;
        }


    }
}