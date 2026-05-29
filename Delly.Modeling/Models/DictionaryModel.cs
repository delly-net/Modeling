using System;
using System.Collections.Generic;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Dictionary&lt;,&gt; 泛型定义模型
    /// </summary>
    public sealed class DictionaryModel : IModel
    {
        private static readonly DictionaryModel _instance = new DictionaryModel();
        private static readonly IModel[] _genericModels = Array.Empty<IModel>();

        /// <summary>
        /// 获取 DictionaryModel 单例实例
        /// </summary>
        public static DictionaryModel Instance => _instance;

        private DictionaryModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => typeof(Dictionary<,>).Name;

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
            throw new ArgumentException("DictionaryModel.CreateInstance 需要使用 MakeGenericModel 创建");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => typeof(Dictionary<,>);

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
            throw new NotSupportedException("DictionaryModel 不支持泛型解析");
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
        public IModelProperty[] GetProperties()
        {
            return Array.Empty<IModelProperty>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IModelProperty GetProperty(string name)
#else
        public IModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型模型的定义模型
        /// </summary>
        /// <returns>自身</returns>
        public IModel GetGenericModelDefinition()
        {
            return this;
        }

        /// <summary>
        /// 获取所有已构造的泛型模型
        /// </summary>
        /// <returns>已构造的泛型模型列表</returns>
        public IReadOnlyList<IModel> GetGenericModels()
        {
            return _genericModels;
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型模型</returns>
        public IModel MakeGenericModel(params IModel[] models)
        {
            if (models == null || models.Length != 2)
            {
                throw new ArgumentException("Dictionary<TKey, TValue> 需要两个泛型参数");
            }

            var keyType = models[0].ClassType;
            var valueType = models[1].ClassType;
            var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dictModel = new ConstructedGenericModel(dictType, this, models);
            return dictModel;
        }
    }
}
