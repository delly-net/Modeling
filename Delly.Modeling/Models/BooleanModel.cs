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

        /// <summary>
        /// 模型类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        public Type ClassType => typeof(bool);

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => args == null || args.Length == 0 ? false : Convert.ToBoolean(args[0]);

        /// <summary>
        /// 将输入对象解析为 Boolean 实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>Boolean 实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj)
#else
        public object Parse(object? obj)
#endif
        {
            var result = TryParse(obj);
            if (result == null)
                throw new ArgumentException($"Cannot convert {obj?.GetType().Name ?? "null"} to Boolean");
            return result;
        }

        /// <summary>
        /// 尝试将输入对象解析为 Boolean 实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>Boolean 实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj)
#else
        public object? TryParse(object? obj)
#endif
        {
            if (obj == null)
                return null;

            if (obj is bool value)
                return value;

            if (obj is string str && bool.TryParse(str, out var boolVal))
                return boolVal;

            return null;
        }
    }
}