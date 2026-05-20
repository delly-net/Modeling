using Delly.Modeling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace Deme;

/// <summary>
/// 用户
/// </summary>
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

