using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 已构造的泛型实体模型
    /// </summary>
    public sealed class ConstructedGenericEntityModel : IEntityModel
    {
        private readonly Type _constructedType;
        private readonly IEntityModel _definition;
        private readonly IEntityModel[] _genericArguments;

        /// <summary>
        /// 创建已构造的泛型实体模型
        /// </summary>
        /// <param name="constructedType">已构造的类型</param>
        /// <param name="definition">泛型定义模型</param>
        /// <param name="genericArguments">泛型参数模型列表</param>
        public ConstructedGenericEntityModel(Type constructedType, IEntityModel definition, IEntityModel[] genericArguments)
        {
            _constructedType = constructedType;
            _definition = definition;
            _genericArguments = genericArguments;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _constructedType.Name;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => _constructedType.Name;

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string Namespace => _constructedType.Namespace ?? string.Empty;

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return Activator.CreateInstance(_constructedType);
            }
            return Activator.CreateInstance(_constructedType, args);
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _constructedType;

        /// <summary>
        /// 将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj)
#else
        public object Parse(object? obj)
#endif
        {
            throw new NotSupportedException("ConstructedGenericEntityModel 不支持泛型解析");
        }

        /// <summary>
        /// 尝试将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj)
#else
        public object? TryParse(object? obj)
#endif
        {
            return null;
        }

        /// <summary>
        /// 是否为值类型对象
        /// </summary>
        public bool IsValue => false;

        /// <summary>
        /// 是否为泛型模型
        /// </summary>
        public bool IsGenericModel => true;

        /// <summary>
        /// 是否为开放泛型定义
        /// </summary>
        public bool IsGenericDefinition => false;

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        public int GenericDefinitionCount => _definition.GenericDefinitionCount;

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        public IEntityModelProperty[] GetProperties()
        {
            return Array.Empty<IEntityModelProperty>();
        }

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>索引定义数组</returns>
        public EntityModelIndex[] GetIndexes()
        {
            return Array.Empty<EntityModelIndex>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IEntityModelProperty GetProperty(string name)
#else
        public IEntityModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型实体模型的定义模型
        /// </summary>
        /// <returns>泛型实体定义模型</returns>
        public IEntityModel GetGenericModelDefinition()
        {
            return _definition;
        }

        /// <summary>
        /// 获取所有已构造的泛型实体模型
        /// </summary>
        /// <returns>已构造的泛型实体模型列表</returns>
        public IReadOnlyList<IEntityModel> GetGenericModels()
        {
            return Array.Empty<IEntityModel>();
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型实体模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型实体模型</returns>
        /// <exception cref="NotSupportedException">已构造的泛型实体模型不支持再次构造</exception>
        public IEntityModel MakeGenericModel(params IEntityModel[] models)
        {
            throw new NotSupportedException("已构造的泛型实体模型不支持再次构造");
        }
    }
}
