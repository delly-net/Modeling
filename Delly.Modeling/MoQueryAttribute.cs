using System;

namespace Delly.Modeling
{
    /// <summary>
    /// 查询对象特性，标记查询类以启用源代码生成
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MoQueryAttribute : Attribute
    {
    }
}