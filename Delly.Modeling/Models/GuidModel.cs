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
    }
}