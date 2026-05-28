using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型化表特性，标记类以启用源代码生成
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MoTableAttribute : TableAttribute
    {
        /// <summary>
        /// 初始化模型化表特性实例
        /// </summary>
        /// <param name="name">表名称</param>
        public MoTableAttribute(string name) : base(name)
        {

        }
    }
}
