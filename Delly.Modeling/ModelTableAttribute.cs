using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型化表特性，标记类以启用源代码生成
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelTableAttribute : TableAttribute
    {
        /// <summary>
        /// 模型化表特性
        /// </summary>
        /// <param name="name"></param>
        public ModelTableAttribute(string name) : base(name)
        {

        }
    }
}
