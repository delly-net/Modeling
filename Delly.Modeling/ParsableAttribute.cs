#if !NETSTANDARD2_0
#nullable enable
#endif

using System;

namespace Delly.Modeling
{
    /// <summary>
    /// 用于标记 IParsable<T> 实现类的特性，供源生成器查找使用
    /// </summary>
    /// <remarks>
    /// 标记此特性的类必须是 IParsable&lt;T&gt; 的实现，其中 T 为目标类型。
    /// 源生成器会根据 IParsable&lt;T&gt; 的泛型类型参数确定目标类型。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ParsableAttribute : Attribute
    {
    }
}