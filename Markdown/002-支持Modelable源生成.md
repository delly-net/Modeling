# 简介

针对Modelable特性进行源生成支持

# 规则

- 在Delly.Modeling.Generator子项目创建源生成支持代码
- 添加针对Modelable特性的源生成能力
- 源生成的代码文件以.Model.g.cs结尾
- 读取目标类的所有属性信息并生成代码

# 目标

- 达到**生成代码示例**中的效果，格式完全一样

# 生成代码示例

```
namespace Deme;

public partial class User
{
    /// <summary>
    /// 获取模型
    /// </summary>
    public static g__UserModel GetModel() => g__UserModel.Instance;
}

/// <summary>
/// 自动模型
/// </summary>
public class g__UserModel : IModel<User>
{

    // 固定静态共享实例
    private static readonly g__UserModel _instance = new g__UserModel();

    /// <summary>
    /// 公共实例
    /// </summary>
    public static g__UserModel Instance => _instance;

    /// <summary>
    /// 获取集合
    /// </summary>
    /// <returns></returns>
    public string[] GetProperties()
    {
        return [nameof(User.Id), nameof(User.Name), nameof(User.Password)];
    }

    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="model"></param>
    /// <param name="name"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public object? GetProperty(User user, string name)
    {
        switch (name)
        {
            case nameof(User.Id):
                return user.Id;
            case nameof(User.Name):
                return user.Name;
            case nameof(User.Password):
                return user.Password;
            default: throw new MissingMemberException($"Property '{name}' not found.");
        }
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="model"></param>
    /// <param name="user"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetProperty(User user, string name, object? value)
    {
        switch (name)
        {
            case nameof(User.Id):
                throw new NotSupportedException($"Property '{name}' is readonly.");
            case nameof(User.Name):
                user.Name = value?.ToString() ?? throw new NoNullAllowedException($"Property '{name}' not allowed to be null.");
                break;
            case nameof(User.Password):
                user.Password = value?.ToString();
                break;
            default: throw new MissingMemberException($"Property '{name}' not found.");
        }
    }
}
```