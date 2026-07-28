using Snet.Windows.Core.mvvm;

namespace Snet.Iot.Debug.model
{
    /// <summary>
    /// OPC UA 节点浏览消息结构体，用于在列表中展示节点的详细属性信息。
    /// </summary>
    public class OpcUaNodeBrowseMessageStructuralBody : BindNotify
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index
        {
            get => GetProperty(() => Index);
            set => SetProperty(() => Index, value);
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => GetProperty(() => Name);
            set => SetProperty(() => Name, value);
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
        /// 值
        /// </summary>
        public string Value
        {
            get => GetProperty(() => Value);
            set => SetProperty(() => Value, value);
        }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type
        {
            get => GetProperty(() => Type);
            set => SetProperty(() => Type, value);
        }

        /// <summary>
        /// 访问级别
        /// </summary>
        public string AccessLevel
        {
            get => GetProperty(() => AccessLevel);
            set => SetProperty(() => AccessLevel, value);
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => GetProperty(() => Description);
            set => SetProperty(() => Description, value);
        }
    }
}