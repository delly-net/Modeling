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

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(object[] args) => args == null || args.Length == 0 ? 0L : Convert.ToInt64(args[0]);
    }
}