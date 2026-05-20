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
using System.ComponentModel.DataAnnotations;

namespace Demo;

/// <summary>
/// 用户
/// </summary>
[MoTable("user")]
public partial class UserEntity(string id)
{
    /// <summary>
    /// Id
    /// </summary>
    [MoColumn("id", Key = true, Comment = "唯一Id")]
    public string Id { get; } = id;

    /// <summary>
    /// 名称
    /// </summary>
    [MoColumnIndex(IsUnique = true)]
    [MoColumnIndex(Name = "NAME_PASSWORD")]
    [MoColumn("name", Comment = "名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [MoColumn(Comment = "密码")]
    [MoColumnIndex(Name = "NAME_PASSWORD")]
    public string? Password { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    [MoColumn("age", Comment = "年龄")]
    public long Age { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [MoColumnIndex]
    [MoColumn("is_deleted", Comment = "是否删除")]
    public bool IsDeleted { get; set; }
}


