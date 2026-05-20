using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象接口，提供属性访问能力
    /// </summary>
    /// <typeparam name="T">模型化的类型</typeparam>
    public interface IModel<T>
    {
        /// <summary>
        /// 获取属性名称集合
        /// </summary>
        /// <returns>属性名称数组</returns>
        string[] GetProperties();

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="name">属性名称</param>
        /// <returns>属性值</returns>
        object GetProperty(T obj, string name);

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="name">属性名称</param>
        /// <param name="value">属性值</param>
        void SetProperty(T obj, string name, object value);
    }
}
