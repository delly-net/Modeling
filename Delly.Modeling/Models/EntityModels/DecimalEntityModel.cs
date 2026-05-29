using System;
using Delly.Modeling.Models;

namespace Delly.Modeling
{
    /// <summary>
    /// Decimal 实体建模适配器
    /// </summary>
    public sealed class DecimalEntityModel : IEntityModel
    {
        private static readonly DecimalEntityModel _instance = new DecimalEntityModel();
        private readonly IBaseModel _baseModel = DecimalModel.Instance;

        /// <summary>
        /// 获取 Decimal 实体建模单例实例
        /// </summary>
        public static DecimalEntityModel Instance => _instance;

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
        public string ClassName => "Decimal";

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
    }
}
