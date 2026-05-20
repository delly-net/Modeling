using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Int64建模
    /// </summary>
    public sealed class Int64Model : IBaseModel
    {
        // 固定静态共享实例
        private static readonly Int64Model _instance = new Int64Model();

        /// <summary>
        /// Int64建模 公共实例
        /// </summary>
        public static Int64Model Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Int64);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);
    }
}