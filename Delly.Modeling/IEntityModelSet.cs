#if !NETSTANDARD2_0
#nullable enable
#endif

namespace Delly.Modeling
{
    /// <summary>
    /// 实体模型集合接口，用于建模信息的自动收集
    /// </summary>
    public interface IEntityModelSet
    {
        /// <summary>
        /// 获取集合中的所有实体模型
        /// </summary>
        /// <returns>所有实体模型</returns>
        System.Collections.Generic.IReadOnlyList<IEntityModel> GetModels();

        /// <summary>
        /// 获取集合中的模型数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取指定类型的模型
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>指定类型的模型</returns>
        /// <exception cref="System.NotSupportedException">当类型不匹配时抛出</exception>
        IEntityModel GetModel<T>() where T : class;

        /// <summary>
        /// 尝试获取指定类型的模型
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>模型对象，未找到时返回 null</returns>
#if !NETSTANDARD2_0
        IEntityModel? TryGetModel<T>() where T : class;
#else
        IEntityModel TryGetModel<T>() where T : class;
#endif
    }
}