using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling.Models
{
    /// <summary>
    /// Int32建模
    /// </summary>
    public sealed class Int32Model : IModel
    {
        // 固定静态共享实例
        private static readonly Int32Model _instance = new Int32Model();

        /// <summary>
        /// Int32建模 公共实例
        /// </summary>
        public static Int32Model Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(Int32);

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
