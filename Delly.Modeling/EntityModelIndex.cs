using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 实体建模索引
    /// </summary>
    public sealed class EntityModelIndex
    {
        /// <summary>
        /// 实体建模索引
        /// </summary>
        public EntityModelIndex(string[] columns, bool isUnique)
        {
            Columns = columns;
            IsUnique = isUnique;
        }

        /// <summary>
        /// 字段集合
        /// </summary>
        public string[] Columns { get; }

        /// <summary>
        /// 唯一索引
        /// </summary>
        public bool IsUnique { get; }
    }
}
