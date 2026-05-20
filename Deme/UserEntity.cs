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
[ModelTable("user")]
public partial class UserEntity(string id)
{
    /// <summary>
    /// Id
    /// </summary>
    [Description("Id")]
    public string Id { get; } = id;

    /// <summary>
    /// 名称
    /// </summary>
    [Description("名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Description("密码")]
    public string? Password { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    [Description("年龄")]
    public long Age { get; set; }
}


