namespace Delly.Modeling
{
    /// <summary>
    /// 建模工具类，提供静态辅助方法
    /// </summary>
    public static class ModelUtils
    {
        /// <summary>
        /// 获取模型的类全称
        /// </summary>
        /// <param name="model">实体模型</param>
        /// <returns>类全称，格式为 {Namespace}.{ClassName}</returns>
        public static string GetFullName(IEntityModel model)
        {
            return $"{model.Namespace}.{model.ClassName}";
        }
    }
}