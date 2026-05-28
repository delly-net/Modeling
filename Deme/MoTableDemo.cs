using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// MoTable 实体模型功能演示
/// </summary>
public class MoTableDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("MoTable 实体模型功能演示");
        Console.WriteLine("============================================\n");

        var userEntity = new UserEntity(Guid.NewGuid().ToString("N"));
        var entityModel = UserEntity.GetEntityModel();

        Console.WriteLine("UserEntity 属性列表:");
        foreach (var property in entityModel.GetProperties())
        {
            Console.WriteLine($"  {property.Name}, {property.Comment}");
        }

        Console.WriteLine("\n============================================");
        Console.WriteLine("MoTable 实体模型功能演示完成");
        Console.WriteLine("============================================");
    }
}
