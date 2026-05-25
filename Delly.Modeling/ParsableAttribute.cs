#if !NETSTANDARD2_0
#nullable enable
#endif

using System;

namespace Delly.Modeling
{
    /// <summary>
    /// 用于标记目标类并指定其 Parser 类型的特性，供源生成器查找使用
    /// </summary>
    /// <remarks>
    /// 标记此特性的目标类可通过源生成器生成的 Parse/TryParse 方法进行解析。
    /// 指定的 Parser 类型必须实现 IParsable&lt;T&gt; 接口，其中 T 为目标类类型。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ParsableAttribute : Attribute
    {
        /// <summary>
        /// Parser 类型
        /// </summary>
        public Type ParserType { get; }

        /// <summary>
        /// 初始化 ParsableAttribute 实例
        /// </summary>
        /// <param name="parserType">Parser 类型，必须实现 IParsable&lt;T&gt; 接口，其中 T 为目标类类型</param>
        public ParsableAttribute(Type parserType)
        {
            ParserType = parserType;
        }
    }
}