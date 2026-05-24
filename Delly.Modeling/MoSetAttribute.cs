using System;

namespace Delly.Modeling
{
    /// <summary>
    /// 建模集合特性，标记类以启用源代码生成自动收集功能
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MoSetAttribute : Attribute
    {
    }
}