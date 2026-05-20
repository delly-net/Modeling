using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delly.Modeling
{
    /// <summary>
    /// 列索引特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MoColumnIndexAttribute : Attribute
    {
        /// <summary>
        /// 唯一索引
        /// </summary>
        public bool IsUnique { get; set; } = false;

        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
