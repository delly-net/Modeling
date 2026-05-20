using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delly.Modeling
{
    /// <summary>
    /// 列索引特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnIndexAttribute : Attribute
    {
        /// <summary>
        /// 唯一索引
        /// </summary>
        public bool IsUnique { get; set; } = false;
    }
}
