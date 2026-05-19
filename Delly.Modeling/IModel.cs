using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IModel<T>
    {
        /// <summary>
        /// 获取获取属性集合
        /// </summary>
        /// <returns></returns>
        string[] GetProperties();

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetProperty(T obj, string name);

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetProperty(T obj, string name, object value);
    }
}
