#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 实体建模属性
    /// </summary>
    public interface IEntityModelProperty : IModelProperty
    {

        /// <summary>
        /// 属性名
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// 是否主键
        /// 来源为[Key]特性定义
        /// </summary>
        bool IsPrimaryKey { get; }

        /// <summary>
        /// 是否自增长
        /// 来源为[DatabaseGenerated(DatabaseGeneratedOption.Identity)]特性定义
        /// </summary>
        bool IsAutoIncrement { get; }

    }
}
