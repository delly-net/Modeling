using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Guid建模
    /// </summary>
    public sealed class GuidModel : IBaseModel
    {
        // 固定静态共享实例
        private static readonly GuidModel _instance = new GuidModel();

        /// <summary>
        /// Guid建模 公共实例
        /// </summary>
        public static GuidModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Guid);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => args == null || args.Length == 0 ? Guid.Empty : (args[0] is Guid guid ? guid : Guid.Parse(args[0]?.ToString() ?? Guid.Empty.ToString()));
    }
}