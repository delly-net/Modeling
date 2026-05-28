using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// Modelable 基础功能演示
/// </summary>
public class ModelableDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("Modelable 基础功能演示");
        Console.WriteLine("============================================\n");

        var user = new User(Guid.NewGuid().ToString("N"));
        var model = User.GetModel();
        var propertyId = model.GetProperty(nameof(User.Id));
        var propertyName = model.GetProperty(nameof(User.Name));

        Console.WriteLine($"Id 属性值: {propertyId?.GetValue(user)}");
        propertyName?.SetValue(user, "model");
        Console.WriteLine($"设置 Name 后的值: {propertyName?.GetValue(user)}");

        Console.WriteLine("\n============================================");
        Console.WriteLine("Modelable 基础功能演示完成");
        Console.WriteLine("============================================");
    }
}
