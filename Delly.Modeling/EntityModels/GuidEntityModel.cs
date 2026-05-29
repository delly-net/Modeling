using System;
using System.Collections.Generic;
using Delly.Modeling.Models;

namespace Delly.Modeling.EntityModels
{
    /// <summary>
    /// Guid 实体建模适配器
    /// </summary>
    public sealed class GuidEntityModel : IEntityModel
    {
        private static readonly GuidEntityModel _instance = new GuidEntityModel();
        private readonly IBaseModel _baseModel = GuidModel.Instance;

        /// <summary>
        /// 获取 Guid 实体建模单例实例
        /// </summary>
        public static GuidEntityModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _baseModel.Name;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => _baseModel.Namespace;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => "Guid";

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _baseModel.ClassType;

        /// <summary>
        /// 是否为值类型对象
        /// </summary>
        public bool IsValue => _baseModel.IsValue;

        /// <summary>
        /// 是否为泛型模型
        /// </summary>
        public bool IsGenericModel => _baseModel.IsGenericModel;

        /// <summary>
        /// 是否为开放泛型定义
        /// </summary>
        public bool IsGenericDefinition => _baseModel.IsGenericDefinition;

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        public int GenericDefinitionCount => _baseModel.GenericDefinitionCount;

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => _baseModel.CreateInstance(args);

        /// <summary>
        /// 将输入对象解析为模型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>模型实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj) => _baseModel.Parse(obj);
#else
        public object Parse(object? obj) => _baseModel.Parse(obj);
#endif

        /// <summary>
        /// 尝试将输入对象解析为模型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>模型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj) => _baseModel.TryParse(obj);
#else
        public object? TryParse(object? obj) => _baseModel.TryParse(obj);
#endif

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>空数组</returns>
        public IEntityModelProperty[] GetProperties() => Array.Empty<IEntityModelProperty>();

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>空数组</returns>
        public EntityModelIndex[] GetIndexes() => Array.Empty<EntityModelIndex>();

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>null</returns>
#if NETSTANDARD2_0
        public IEntityModelProperty GetProperty(string name) => null;
#else
        public IEntityModelProperty? GetProperty(string name) => null;
#endif

        /// <summary>
        /// 获取泛型实体模型的定义模型
        /// </summary>
        /// <returns>自身</returns>
        public IEntityModel GetGenericModelDefinition()
        {
            return this;
        }

        /// <summary>
        /// 获取所有已构造的泛型实体模型
        /// </summary>
        /// <returns>空列表</returns>
        public IReadOnlyList<IEntityModel> GetGenericModels()
        {
            return Array.Empty<IEntityModel>();
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型实体模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型实体模型</returns>
        /// <exception cref="NotSupportedException">Guid 不支持泛型实体建模创建</exception>
        public IEntityModel MakeGenericModel(params IEntityModel[] models)
        {
            throw new NotSupportedException("Guid 不支持泛型实体建模创建");
        }
    }
}
