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

        /// <summary>
        /// 是否为值类型对象，基础值类型和 string 为 true，其他 class 类型为 false
        /// </summary>
        bool IsValue { get; }

        /// <summary>
        /// 是否为泛型模型
        /// </summary>
        /// <remarks>
        /// 对于开放泛型定义（如 List&lt;&gt;）返回 true
        /// 对于已构造泛型（如 List&lt;int&gt;）返回 true
        /// 对于非泛型类型（如 int、string）返回 false
        /// </remarks>
        bool IsGenericModel { get; }

        /// <summary>
        /// 是否为开放泛型定义
        /// </summary>
        /// <remarks>
        /// 对于开放泛型定义（如 List&lt;&gt;、Dictionary&lt;,&gt;）返回 true
        /// 对于已构造泛型和非泛型类型返回 false
        /// </remarks>
        bool IsGenericDefinition { get; }

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        /// <remarks>
        /// List&lt;&gt; 返回 1，Dictionary&lt;,&gt; 返回 2
        /// 非泛型类型返回 0
        /// </remarks>
        int GenericDefinitionCount { get; }

    }
}
