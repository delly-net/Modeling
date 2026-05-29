using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象接口，提供属性访问能力
    /// </summary>
    public interface IModel : IBaseModel
    {

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

        /// <summary>
        /// 获取泛型模型的定义模型
        /// </summary>
        /// <returns>泛型定义模型，对于非泛型模型返回自身</returns>
        IModel GetGenericModelDefinition();

        /// <summary>
        /// 获取所有已构造的泛型模型
        /// </summary>
        /// <returns>已构造的泛型模型列表</returns>
        IReadOnlyList<IModel> GetGenericModels();

        /// <summary>
        /// 根据泛型参数创建已构造的泛型模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型模型</returns>
        IModel MakeGenericModel(params IModel[] models);

    }
}
