using Delly.Modeling;

namespace Demo;

/// <summary>
/// 用户查询对象
/// </summary>
[MoQuery]
public partial class UserQuery
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 最小年龄
    /// </summary>
    public int? MinAge { get; set; }

    /// <summary>
    /// 最大年龄
    /// </summary>
    public int? MaxAge { get; set; }

    /// <summary>
    /// 起始时间
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
}