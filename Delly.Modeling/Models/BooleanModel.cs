using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Boolean建模
    /// </summary>
    public sealed class BooleanModel : IModel
    {
        // 固定静态共享实例
        private static readonly BooleanModel _instance = new BooleanModel();

        /// <summary>
        /// Boolean建模 公共实例
        /// </summary>
        public static BooleanModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Boolean);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);

        /// <summary>
        /// 模型类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        public Type ClassType => typeof(bool);

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => args == null || args.Length == 0 ? false : Convert.ToBoolean(args[0]);

        /// <summary>
        /// 将输入对象解析为 Boolean 实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>Boolean 实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj)
#else
        public object Parse(object? obj)
#endif
        {
            var result = TryParse(obj);
            if (result == null)
                throw new ArgumentException($"Cannot convert {obj?.GetType().Name ?? "null"} to Boolean");
            return result;
        }

        /// <summary>
        /// 尝试将输入对象解析为 Boolean 实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>Boolean 实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj)
#else
        public object? TryParse(object? obj)
#endif
        {
            if (obj == null)
                return null;

            if (obj is bool value)
                return value;

            if (obj is string str && bool.TryParse(str, out var boolVal))
                return boolVal;

            return null;
        }

        /// <summary>
        /// 是否为值类型对象
        /// </summary>
        public bool IsValue => true;

        /// <summary>
        /// 是否为泛型模型
        /// </summary>
        public bool IsGenericModel => false;

        /// <summary>
        /// 是否为开放泛型定义
        /// </summary>
        public bool IsGenericDefinition => false;

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        public int GenericDefinitionCount => 0;

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
        /// <returns>空列表</returns>
        public IReadOnlyList<IModel> GetGenericModels()
        {
            return Array.Empty<IModel>();
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型模型</returns>
        /// <exception cref="NotSupportedException">BooleanModel 不支持泛型建模创建</exception>
        public IModel MakeGenericModel(params IModel[] models)
        {
            throw new NotSupportedException("BooleanModel 不支持泛型建模创建");
        }
    }
}