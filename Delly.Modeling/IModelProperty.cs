#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象属性接口，提供属性访问能力
    /// </summary>
    public interface IModelProperty
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 属性建模
        /// </summary>
        IBaseModel PropertyModel { get; }

        /// <summary>
        /// 属性类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <returns>属性值</returns>
#if NETSTANDARD2_0
        object GetValue(object obj);
#else
        object? GetValue(object? obj);
#endif

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">属性值</param>
#if NETSTANDARD2_0
        void SetValue(object obj, object value);
#else
        void SetValue(object? obj, object? value);
#endif

    }
}
