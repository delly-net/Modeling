using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delly.Modeling
{
    /// <summary>
    /// 表索引特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableIndexAttribute : Attribute
    {
        /// <summary>
        /// 表索引特性
        /// </summary>
        /// <param name="columns"></param>
        public TableIndexAttribute(params string[] columns)
        {
            Columns = columns;
        }

        /// <summary>
        /// 字段集合
        /// </summary>
        public string[] Columns { get; }

        /// <summary>
        /// 唯一索引
        /// </summary>
        public bool IsUnique { get; set; } = false;
    }
}
