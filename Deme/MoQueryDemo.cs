using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// MoQuery 查询模型功能演示
/// </summary>
public class MoQueryDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("MoQuery 查询模型功能演示");
        Console.WriteLine("============================================\n");

        var userQueryModel = UserQuery.GetEntityModel();

        Console.WriteLine("UserQuery 模型信息:");
        Console.WriteLine($"  表名: {userQueryModel.Name}");
        Console.WriteLine($"  类名: {userQueryModel.ClassName}");
        Console.WriteLine($"  命名空间: {userQueryModel.Namespace}");
        Console.WriteLine($"  属性数量: {userQueryModel.GetProperties().Length}");

        Console.WriteLine("\nUserQuery 属性列表:");
        foreach (var property in userQueryModel.GetProperties())
        {
            Console.WriteLine($"  {property.Name} ({property.Type}): {property.PropertyName}");
        }

        Console.WriteLine("\n============================================");
        Console.WriteLine("MoQuery 查询模型功能演示完成");
        Console.WriteLine("============================================");
    }
}
