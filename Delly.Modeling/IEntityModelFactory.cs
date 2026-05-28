#if !NETSTANDARD2_0
#nullable enable
#endif

using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 实体建模工厂接口，定义实体建模管理的标准
    /// </summary>
    public interface IEntityModelFactory
    {
        /// <summary>
        /// 获取指定类全称的实体模型
        /// </summary>
        /// <param name="fullName">类全称，格式为 {Namespace}.{ClassName}</param>
        /// <returns>实体模型实例，如不存在则返回 null</returns>
#if NETSTANDARD2_0
        IEntityModel GetModel(string fullName);
#else
        IEntityModel? GetModel(string fullName);
#endif

        /// <summary>
        /// 获取指定表名的实体模型
        /// </summary>
        /// <param name="tableName">表名/查询对象名称</param>
        /// <returns>实体模型实例，如不存在则返回 null</returns>
#if NETSTANDARD2_0
        IEntityModel GetModelByTableName(string tableName);
#else
        IEntityModel? GetModelByTableName(string tableName);
#endif

        /// <summary>
        /// 获取所有实体模型
        /// </summary>
        /// <returns>所有已注册的实体模型</returns>
        IEnumerable<IEntityModel> GetAllModels();

        /// <summary>
        /// 获取指定命名空间的所有实体模型
        /// </summary>
        /// <param name="ns">命名空间</param>
        /// <returns>指定命名空间的所有实体模型</returns>
        IEnumerable<IEntityModel> GetModelsByNamespace(string ns);

        /// <summary>
        /// 检查是否存在指定类全称的实体模型
        /// </summary>
        /// <param name="fullName">类全称</param>
        /// <returns>是否存在</returns>
        bool HasModel(string fullName);

        /// <summary>
        /// 获取模型总数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加建模集合
        /// </summary>
        /// <param name="modelSet">建模集合</param>
        void AddSet(IEntityModelSet modelSet);

        /// <summary>
        /// 添加单个实体模型
        /// </summary>
        /// <param name="model">实体模型</param>
        void Add(IEntityModel model);

        /// <summary>
        /// 获取指定类型的实体模型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>指定类型的实体模型</returns>
        /// <exception cref="System.NotSupportedException">当类型未在任何已注册集合中找到时抛出</exception>
        IEntityModel GetModel<T>();

        /// <summary>
        /// 尝试获取指定类型的实体模型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>指定类型的实体模型，未找到时返回 null</returns>
#if NETSTANDARD2_0
        IEntityModel TryGetModel<T>();
#else
        IEntityModel? TryGetModel<T>();
#endif
    }
}