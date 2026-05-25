using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// DateTime建模
    /// </summary>
    public sealed class DateTimeModel : IBaseModel
    {
        // 固定静态共享实例
        private static readonly DateTimeModel _instance = new DateTimeModel();

        /// <summary>
        /// DateTime建模 公共实例
        /// </summary>
        public static DateTimeModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(DateTime);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => args == null || args.Length == 0 ? default(DateTime) : Convert.ToDateTime(args[0]);
    }
}