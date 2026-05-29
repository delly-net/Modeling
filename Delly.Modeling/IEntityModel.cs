using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 实体模型
    /// </summary>
    public interface IEntityModel : IBaseModel
    {
        /// <summary>
        /// 类名
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        IEntityModelProperty[] GetProperties();

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>属性对象数组</returns>
        EntityModelIndex[] GetIndexes();

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        IEntityModelProperty GetProperty(string name);
#else
        IEntityModelProperty? GetProperty(string name);
#endif

        /// <summary>
        /// 获取泛型实体模型的定义模型
        /// </summary>
        /// <returns>泛型实体定义模型，对于非泛型实体模型返回自身</returns>
        IEntityModel GetGenericModelDefinition();

        /// <summary>
        /// 获取所有已构造的泛型实体模型
        /// </summary>
        /// <returns>已构造的泛型实体模型列表</returns>
        IReadOnlyList<IEntityModel> GetGenericModels();

        /// <summary>
        /// 根据泛型参数创建已构造的泛型实体模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型实体模型</returns>
        IEntityModel MakeGenericModel(params IEntityModel[] models);
    }
}
