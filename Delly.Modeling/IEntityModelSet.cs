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
    }
}