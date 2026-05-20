using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Double建模
    /// </summary>
    public sealed class DoubleModel : IBaseModel
    {
        // 固定静态共享实例
        private static readonly DoubleModel _instance = new DoubleModel();

        /// <summary>
        /// Double建模 公共实例
        /// </summary>
        public static DoubleModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Double);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);
    }
}