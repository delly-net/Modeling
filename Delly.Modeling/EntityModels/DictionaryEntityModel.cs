using System;
using System.Collections.Generic;

namespace Delly.Modeling.EntityModels
{
    /// <summary>
    /// Dictionary&lt;,&gt; 泛型实体定义模型
    /// </summary>
    public sealed class DictionaryEntityModel : IEntityModel
    {
        private static readonly DictionaryEntityModel _instance = new DictionaryEntityModel();
        private static readonly Type _dictionaryType = typeof(Dictionary<,>);
        private static readonly List<IEntityModel> _genericModels = new List<IEntityModel>();

        /// <summary>
        /// 获取 DictionaryEntityModel 单例实例
        /// </summary>
        public static DictionaryEntityModel Instance => _instance;

        private DictionaryEntityModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _dictionaryType.Name;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => _dictionaryType.Name;

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string Namespace => "System.Collections.Generic";

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args)
        {
            throw new ArgumentException("DictionaryEntityModel.CreateInstance 需要使用 MakeGenericModel 创建");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _dictionaryType;

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
            throw new NotSupportedException("DictionaryEntityModel 不支持泛型解析");
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
        public bool IsGenericDefinition => true;

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        public int GenericDefinitionCount => 2;

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
        /// <returns>自身</returns>
        public IEntityModel GetGenericModelDefinition()
        {
            return this;
        }

        /// <summary>
        /// 获取所有已构造的泛型实体模型
        /// </summary>
        /// <returns>已构造的泛型实体模型列表</returns>
        public IReadOnlyList<IEntityModel> GetGenericModels()
        {
            return _genericModels;
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型实体模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型实体模型</returns>
        public IEntityModel MakeGenericModel(params IEntityModel[] models)
        {
            if (models == null || models.Length != 2)
            {
                throw new ArgumentException("Dictionary<TKey, TValue> 需要两个泛型参数");
            }

            var keyType = models[0].ClassType;
            var valueType = models[1].ClassType;
            var dictType = _dictionaryType.MakeGenericType(keyType, valueType);
            var dictModel = new ConstructedGenericEntityModel(dictType, this, models);
            _genericModels.Add(dictModel);
            return dictModel;
        }
    }
}
