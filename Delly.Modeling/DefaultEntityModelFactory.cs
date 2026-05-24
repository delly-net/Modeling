#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace Delly.Modeling
{
    /// <summary>
    /// 默认实体建模工厂实现
    /// </summary>
    public sealed class DefaultEntityModelFactory : IEntityModelFactory
    {
        private readonly Dictionary<string, IEntityModel> _models;
        private readonly List<IEntityModelSet> _modelSets;
        private readonly HashSet<IEntityModelSet> _registeredSets;

        /// <summary>
        /// 初始化默认实体建模工厂
        /// </summary>
        public DefaultEntityModelFactory()
        {
            _models = new Dictionary<string, IEntityModel>();
            _modelSets = new List<IEntityModelSet>();
            _registeredSets = new HashSet<IEntityModelSet>();
        }

        /// <summary>
        /// 获取指定类全称的实体模型
        /// </summary>
        /// <param name="fullName">类全称，格式为 {Namespace}.{ClassName}</param>
        /// <returns>实体模型实例，如不存在则返回 null</returns>
#if NETSTANDARD2_0
        public IEntityModel GetModel(string fullName)
#else
        public IEntityModel? GetModel(string fullName)
#endif
        {
            return _models.TryGetValue(fullName, out var model) ? model : null;
        }

        /// <summary>
        /// 获取指定表名的实体模型
        /// </summary>
        /// <param name="tableName">表名/查询对象名称</param>
        /// <returns>实体模型实例，如不存在则返回 null</returns>
#if NETSTANDARD2_0
        public IEntityModel GetModelByTableName(string tableName)
#else
        public IEntityModel? GetModelByTableName(string tableName)
#endif
        {
            return _models.Values.FirstOrDefault(m => m.Name == tableName);
        }

        /// <summary>
        /// 获取所有实体模型
        /// </summary>
        /// <returns>所有已注册的实体模型</returns>
        public IEnumerable<IEntityModel> GetAllModels()
        {
            return _models.Values;
        }

        /// <summary>
        /// 获取指定命名空间的所有实体模型
        /// </summary>
        /// <param name="ns">命名空间</param>
        /// <returns>指定命名空间的所有实体模型</returns>
        public IEnumerable<IEntityModel> GetModelsByNamespace(string ns)
        {
            return _models.Values.Where(m => m.Namespace == ns);
        }

        /// <summary>
        /// 检查是否存在指定类全称的实体模型
        /// </summary>
        /// <param name="fullName">类全称</param>
        /// <returns>是否存在</returns>
        public bool HasModel(string fullName)
        {
            return _models.ContainsKey(fullName);
        }

        /// <summary>
        /// 获取模型总数
        /// </summary>
        public int Count => _models.Count;

        /// <summary>
        /// 添加建模集合
        /// </summary>
        /// <param name="modelSet">建模集合</param>
        public void AddSet(IEntityModelSet modelSet)
        {
            if (_registeredSets.Add(modelSet))
            {
                _modelSets.Add(modelSet);
            }
            var models = modelSet.GetModels();
            foreach (var model in models)
            {
                var fullName = $"{model.Namespace}.{model.ClassName}";
                _models[fullName] = model;
            }
        }

        /// <summary>
        /// 添加单个实体模型
        /// </summary>
        /// <param name="model">实体模型</param>
        public void Add(IEntityModel model)
        {
            var fullName = $"{model.Namespace}.{model.ClassName}";
            _models[fullName] = model;
        }

        /// <summary>
        /// 获取指定类型的实体模型
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>指定类型的实体模型</returns>
        /// <exception cref="System.NotSupportedException">当类型未在任何已注册集合中找到时抛出</exception>
        public IEntityModel GetModel<T>() where T : class
        {
            foreach (var set in _modelSets)
            {
                var model = set.TryGetModel<T>();
                if (model != null)
                {
                    return model;
                }
            }
            throw new NotSupportedException($"类型 {typeof(T).FullName} 未在已注册的建模集合中找到");
        }

        /// <summary>
        /// 尝试获取指定类型的实体模型
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>指定类型的实体模型，未找到时返回 null</returns>
#if NETSTANDARD2_0
        public IEntityModel TryGetModel<T>() where T : class
#else
        public IEntityModel? TryGetModel<T>() where T : class
#endif
        {
            foreach (var set in _modelSets)
            {
                var model = set.TryGetModel<T>();
                if (model != null)
                {
                    return model;
                }
            }
            return null;
        }
    }
}