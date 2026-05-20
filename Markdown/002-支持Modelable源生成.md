# 简介

针对Modelable特性进行源生成支持

# 规则

- 在Delly.Modeling.Generator子项目创建源生成支持代码
- 添加针对Modelable特性的源生成能力
- 源生成的代码文件以.Model.g.cs结尾
- 读取目标类的所有属性信息并生成代码
- 需要兼容不同数据类型进行实现
- 可读可写属性需要独立兼容
- 生成的注释要使用中文

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
public class p__UserModel_Id : IModelProperty
{
    // 固定静态共享实例
    private static readonly p__UserModel_Id _instance = new p__UserModel_Id();

    /// <summary>
    /// 公共实例
    /// </summary>
    public static p__UserModel_Id Instance => _instance;

    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name => nameof(User.Id);

    /// <summary>
    /// 属性建模
    /// </summary>
    public IModel PropertyModel => StringModel.Instance;

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public object? GetValue(object? obj)
    {
        if (obj is null) { return null; }
        if (obj is User user) { return user.Name; }
        return null;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    /// <exception cref="NotSupportedException"></exception>
    public void SetValue(object? obj, object? value)
    {
        throw new NotSupportedException($"Property 'Id' is readonly.");
    }
}

/// <summary>
/// 自动模型
/// </summary>
public class p__UserModel_Age : IModelProperty
{
    // 固定静态共享实例
    private static readonly p__UserModel_Age _instance = new p__UserModel_Age();

    /// <summary>
    /// 公共实例
    /// </summary>
    public static p__UserModel_Age Instance => _instance;

    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name => nameof(User.Age);

    /// <summary>
    /// 属性建模
    /// </summary>
    public IModel PropertyModel => Int64Model.Instance;

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public object? GetValue(object? obj)
    {
        if (obj is null) { return null; }
        if (obj is User user) { return user.Age; }
        return null;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public void SetValue(object? obj, object? value)
    {
        if (obj is null) { throw new NoNullAllowedException($"Target object not allowed to be null."); }
        var user = (User)obj;
        user.Age = Convert.ToInt64(value);
    }
}

/// <summary>
/// 自动模型
/// </summary>
public class c__UserModel : IModel
{

    // 固定静态共享实例
    private static readonly g__UserModel _instance = new g__UserModel();

    /// <summary>
    /// 公共实例
    /// </summary>
    public static g__UserModel Instance => _instance;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name => "UserModel";

    /// <summary>
    /// 命名空间
    /// </summary>
    public string Namespace => "Deme";

    /// <summary>
    /// 获取集合
    /// </summary>
    /// <returns></returns>
    public IModelProperty[] GetProperties()
    {
        return [p__UserModel_Id.Instance];
    }

    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IModelProperty? GetProperty(string name)
    {
        switch (name)
        {
            case nameof(User.Id):
                return p__UserModel_Id.Instance;
            case nameof(User.Age):
                return p__UserModel_Age.Instance;
            default:
                return null;
        }
    }
}
```