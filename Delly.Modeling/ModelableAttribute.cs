using System;

namespace Delly.Modeling
{
    /// <summary>
    /// 可模型化特性，标记类以启用源代码生成
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelableAttribute : Attribute
    {

    }
}
