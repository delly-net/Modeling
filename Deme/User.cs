using Delly.Modeling;
using Delly.Modeling.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo;

/// <summary>
/// 用户
/// </summary>
[Parsable(typeof(UserParser))]
[Modelable]
public partial class User(string id)
{
    /// <summary>
    /// Id
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    public long Age { get; set; }
}

/// <summary>
/// 用户解析器
/// </summary>
public class UserParser : Delly.Modeling.IParsable<User>
{
    /// <summary>
    /// 尝试将输入对象解析为 User 实例
    /// </summary>
    /// <param name="obj">输入对象</param>
    /// <returns>User 实例，解析失败时返回 null</returns>
    public object? TryParse(object? obj)
    {
        if (obj == null)
        {
            return null;
        }

        if (obj is User user)
        {
            return user;
        }

        if (obj is string str && Guid.TryParse(str, out var guid))
            return new User(guid.ToString("N"));

        return null;
    }
}


