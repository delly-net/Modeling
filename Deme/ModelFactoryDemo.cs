using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// 建模工厂功能演示
/// </summary>
public class ModelFactoryDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("建模工厂功能演示");
        Console.WriteLine("============================================\n");

        // 创建工厂实例
        var factory = new DefaultEntityModelFactory();

        // 方式1：直接添加单个模型
        factory.Add(UserEntity.GetEntityModel());
        factory.Add(UserQuery.GetEntityModel());

        Console.WriteLine($"已注册模型总数: {factory.Count}");

        // 获取模型（通过类全称）
        var userEntityModel = factory.GetModel("Demo.UserEntity");
        if (userEntityModel != null)
        {
            Console.WriteLine($"\n通过类全称获取模型: {userEntityModel.ClassName}");
            Console.WriteLine($"  表名: {userEntityModel.Name}");
        }

        // 获取模型（通过表名）
        var byTableName = factory.GetModelByTableName("user");
        if (byTableName != null)
        {
            Console.WriteLine($"\n通过表名获取模型: {byTableName.ClassName}");
        }

        // 获取所有模型
        Console.WriteLine("\n所有已注册模型:");
        foreach (var m in factory.GetAllModels())
        {
            Console.WriteLine($"  {ModelUtils.GetFullName(m)} -> {m.Name}");
        }

        // 获取指定命名空间的模型
        Console.WriteLine("\n命名空间 'Demo' 中的模型:");
        foreach (var m in factory.GetModelsByNamespace("Demo"))
        {
            Console.WriteLine($"  {m.ClassName}");
        }

        // 检查模型是否存在
        Console.WriteLine($"\n是否存在 Demo.UserEntity: {factory.HasModel("Demo.UserEntity")}");
        Console.WriteLine($"是否存在 Demo.NotExist: {factory.HasModel("Demo.NotExist")}");

        Console.WriteLine("\n============================================");
        Console.WriteLine("建模工厂功能演示完成");
        Console.WriteLine("============================================");
    }
}
