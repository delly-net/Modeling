using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Int32建模
    /// </summary>
    public sealed class Int32Model : IBaseModel
    {
        // 固定静态共享实例
        private static readonly Int32Model _instance = new Int32Model();

        /// <summary>
        /// Int32建模 公共实例
        /// </summary>
        public static Int32Model Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Int32);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);

        /// <summary>
        /// 模型类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        public Type ClassType => typeof(int);

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => args == null || args.Length == 0 ? 0 : Convert.ToInt32(args[0]);
    }
}
