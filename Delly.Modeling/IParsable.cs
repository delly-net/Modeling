namespace Delly.Modeling
{
    /// <summary>
    /// 可分析类型接口，用于提供类型解析能力
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <remarks>
    /// 实现此接口的类型为 T 提供解析能力。实现类必须提供无参构造函数，
    /// 并通过在目标类上标记 [Parsable(typeof(ParserClass))] 特性来关联。
    /// 接口中包含一个 TryParse 实例方法。
    /// </remarks>
    public interface IParsable<T>
    {
        /// <summary>
        /// 尝试将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        object TryParse(object obj);
#else
        object? TryParse(object? obj);
#endif
    }
}