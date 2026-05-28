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
        /// 初始化实体建模索引实例
        /// </summary>
        /// <param name="name">索引名称</param>
        /// <param name="columns">字段集合</param>
        /// <param name="isUnique">是否唯一索引</param>
        public EntityModelIndex(string name, string[] columns, bool isUnique)
        {
            Name = name;
            Columns = columns;
            IsUnique = isUnique;
        }

        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name { get; }

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
