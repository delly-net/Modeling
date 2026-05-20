using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象接口，提供属性访问能力
    /// </summary>
    public interface IModel
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
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        IModelProperty[] GetProperties();

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        IModelProperty GetProperty(string name);
#else
        IModelProperty? GetProperty(string name);
#endif

    }
}
