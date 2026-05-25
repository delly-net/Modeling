#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象接口，提供属性访问能力
    /// </summary>
    public interface IBaseModel
    {

        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 类型全名称
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        object CreateInstance(params object[] args);

        /// <summary>
        /// 模型类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        Type ClassType { get; }

        /// <summary>
        /// 将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例</returns>
        /// <exception cref="ArgumentException">当对象无法转换为目标类型时抛出</exception>
#if NETSTANDARD2_0
        object Parse(object obj);
#else
        object Parse(object? obj);
#endif

        /// <summary>
        /// 尝试将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        object TryParse(object obj);
#else
        object? TryParse(object? obj);
#endif

    }
}
