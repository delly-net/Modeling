using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Boolean建模
    /// </summary>
    public sealed class BooleanModel : IBaseModel
    {
        // 固定静态共享实例
        private static readonly BooleanModel _instance = new BooleanModel();

        /// <summary>
        /// Boolean建模 公共实例
        /// </summary>
        public static BooleanModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Boolean);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);
    }
}