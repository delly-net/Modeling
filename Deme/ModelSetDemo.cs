using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// 建模集合功能演示
/// </summary>
public class ModelSetDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("建模集合泛型方法演示");
        Console.WriteLine("============================================\n");

        var entitySet = new EntityModelSet();

        // 测试 GetModels（非泛型方法）
        Console.WriteLine($"GetModels() 返回模型数: {entitySet.GetModels().Count}");
        Console.WriteLine($"Count 属性: {entitySet.Count}");

        // 测试泛型方法
        var userEntityModel2 = entitySet.GetModel<UserEntity>();
        if (userEntityModel2 != null)
        {
            Console.WriteLine($"GetModel<UserEntity> 成功: {userEntityModel2.ClassName}");
        }

        var userQueryModel2 = entitySet.GetModel<UserQuery>();
        if (userQueryModel2 != null)
        {
            Console.WriteLine($"GetModel<UserQuery> 成功: {userQueryModel2.ClassName}");
        }

        // 测试 TryGetModel
        var userEntityModel3 = entitySet.TryGetModel<UserEntity>();
        if (userEntityModel3 != null)
        {
            Console.WriteLine($"TryGetModel<UserEntity> 成功: {userEntityModel3.ClassName}");
        }

        var notExistModel = entitySet.TryGetModel<ModuleA>();
        if (notExistModel == null)
        {
            Console.WriteLine("TryGetModel<ModuleA> 返回 null（符合预期）");
        }

        // 测试 GetModel 对不存在的类型抛出异常
        try
        {
            entitySet.GetModel<ModuleA>();
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"GetModel<ModuleA> 抛出 NotSupportedException: {ex.Message}");
        }

        Console.WriteLine("\n============================================");
        Console.WriteLine("建模集合泛型方法演示完成");
        Console.WriteLine("============================================");
    }
}
