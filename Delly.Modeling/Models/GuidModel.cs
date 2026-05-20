using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Guid建模
    /// </summary>
    public sealed class GuidModel : IModel
    {
        // 固定静态共享实例
        private static readonly GuidModel _instance = new GuidModel();

        /// <summary>
        /// Guid建模 公共实例
        /// </summary>
        public static GuidModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Guid);

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => nameof(System);

        /// <summary>
        /// 属性集合
        /// </summary>
        /// <returns></returns>
        public IModelProperty[] GetProperties()
        {
            return new IModelProperty[0];
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IModelProperty GetProperty(string name)
        {
            return null;
        }
    }
}