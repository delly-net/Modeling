using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型化列特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MoColumnAttribute : ColumnAttribute
    {
        /// <summary>
        /// 初始化模型化列特性实例
        /// </summary>
        public MoColumnAttribute()
        {

        }

        /// <summary>
        /// 初始化模型化列特性实例
        /// </summary>
        /// <param name="name">列名称</param>
        public MoColumnAttribute(string name) : base(name)
        {

        }

        /// <summary>
        /// 类型
        /// </summary>
        public ColumnType Type { get; set; } = ColumnType.UNSET;

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// 精度
        /// </summary>
        public int Precision { get; set; } = 0;

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool Key { get; set; } = false;

        /// <summary>
        /// 是否自增长
        /// </summary>
        public bool AutoIncrement { get; set; } = false;

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; } = string.Empty;

    }
}
