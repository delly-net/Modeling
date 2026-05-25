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
        /// 模型类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        public Type ClassType => typeof(long);

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => args == null || args.Length == 0 ? 0L : Convert.ToInt64(args[0]);

        /// <summary>
        /// 将输入对象解析为 Int64 实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>Int64 实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj)
#else
        public object Parse(object? obj)
#endif
        {
            var result = TryParse(obj);
            if (result == null)
                throw new ArgumentException($"Cannot convert {obj?.GetType().Name ?? "null"} to Int64");
            return result;
        }

        /// <summary>
        /// 尝试将输入对象解析为 Int64 实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>Int64 实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj)
#else
        public object? TryParse(object? obj)
#endif
        {
            if (obj == null)
                return null;

            if (obj is long value)
                return value;

            if (obj is string str && long.TryParse(str, out var longVal))
                return longVal;

            if (obj is int intVal)
                return (long)intVal;

            return null;
        }
    }
}