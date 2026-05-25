using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// CreateInstance 功能演示
/// </summary>
public class CreateInstanceDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("CreateInstance 功能演示（params 参数）");
        Console.WriteLine("============================================\n");

        // ============================================
        // Modelable 类示例
        // ============================================

        Console.WriteLine("1. Modelable 类 (User):");
        Console.WriteLine("----------------------------------------");
        var userModel = User.GetModel();
        Console.WriteLine($"  模型名称: {userModel.Name}");
        Console.WriteLine($"  命名空间: {userModel.Namespace}");

        // 带参数创建（User 有一个参数构造函数）
        var userId = Guid.NewGuid().ToString("N");
        var user1 = (User)userModel.CreateInstance(userId);
        Console.WriteLine($"  带参数创建 user1: Id={user1.Id}, Name={user1.Name}");

        var user2 = (User)userModel.CreateInstance(Guid.NewGuid().ToString("N"));
        user2.Name = "张三";
        user2.Age = 25;
        Console.WriteLine($"  带参数创建 user2: Id={user2.Id}, Name={user2.Name}, Age={user2.Age}");

        Console.WriteLine($"  user1 == user2: {ReferenceEquals(user1, user2)}");

        // ============================================
        // MoTable 类示例
        // ============================================

        Console.WriteLine("\n2. MoTable 类 (UserEntity):");
        Console.WriteLine("----------------------------------------");
        var entityModel = UserEntity.GetEntityModel();
        Console.WriteLine($"  模型名称: {entityModel.Name}");
        Console.WriteLine($"  类名: {entityModel.ClassName}");

        // 带参数创建（UserEntity 有一个参数构造函数）
        var entityId = Guid.NewGuid().ToString("N");
        var entityUser1 = (UserEntity)entityModel.CreateInstance(entityId);
        Console.WriteLine($"  带参数创建 entityUser1: Id={entityUser1.Id}, Name={entityUser1.Name}");

        var entityUser2 = (UserEntity)entityModel.CreateInstance(Guid.NewGuid().ToString("N"));
        entityUser2.Name = "李四";
        entityUser2.Age = 30;
        Console.WriteLine($"  带参数创建 entityUser2: Id={entityUser2.Id}, Name={entityUser2.Name}, Age={entityUser2.Age}");

        Console.WriteLine($"  entityUser1 == entityUser2: {ReferenceEquals(entityUser1, entityUser2)}");

        // ============================================
        // MoQuery 类示例
        // ============================================

        Console.WriteLine("\n3. MoQuery 类 (UserQuery):");
        Console.WriteLine("----------------------------------------");
        var queryModel = UserQuery.GetEntityModel();
        Console.WriteLine($"  模型名称: {queryModel.Name}");
        Console.WriteLine($"  类名: {queryModel.ClassName}");

        // 无参创建（UserQuery 有无参构造函数，使用 params 可以直接调用）
        var query1 = (UserQuery)queryModel.CreateInstance();
        Console.WriteLine($"  无参创建 query1: Username={query1.Username}, MinAge={query1.MinAge}");

        // 修改属性
        query1.Username = "王五";
        query1.MinAge = 18;
        query1.MaxAge = 50;
        Console.WriteLine($"  修改后 query1: Username={query1.Username}, MinAge={query1.MinAge}, MaxAge={query1.MaxAge}");

        // 创建另一个实例
        var query2 = (UserQuery)queryModel.CreateInstance();
        query2.Username = "赵六";
        Console.WriteLine($"  无参创建 query2: Username={query2.Username}");

        Console.WriteLine($"  query1 == query2: {ReferenceEquals(query1, query2)}");

        // ============================================
        // 批量创建示例
        // ============================================

        Console.WriteLine("\n4. 批量创建示例:");
        Console.WriteLine("----------------------------------------");
        var users = new List<User>();
        for (int i = 0; i < 5; i++)
        {
            var id = Guid.NewGuid().ToString("N");
            var user = (User)userModel.CreateInstance(id);
            user.Name = $"用户{i + 1}";
            user.Age = 20 + i;
            users.Add(user);
            Console.WriteLine($"  用户{i + 1}: {user.Name}, Age={user.Age}");
        }

        // ============================================
        // 内置模型类示例
        // ============================================

        Console.WriteLine("\n5. 内置模型类 CreateInstance:");
        Console.WriteLine("----------------------------------------");
        var stringModel = Delly.Modeling.Models.StringModel.Instance;
        var int32Model = Delly.Modeling.Models.Int32Model.Instance;
        var int64Model = Delly.Modeling.Models.Int64Model.Instance;
        var booleanModel = Delly.Modeling.Models.BooleanModel.Instance;

        Console.WriteLine($"  StringModel.CreateInstance(): '{stringModel.CreateInstance()}'");
        Console.WriteLine($"  StringModel.CreateInstance(\"Hello\"): '{stringModel.CreateInstance("Hello")}'");
        Console.WriteLine($"  Int32Model.CreateInstance(): {int32Model.CreateInstance()}");
        Console.WriteLine($"  Int32Model.CreateInstance(\"42\"): {int32Model.CreateInstance("42")}");
        Console.WriteLine($"  Int64Model.CreateInstance(): {int64Model.CreateInstance()}");
        Console.WriteLine($"  Int64Model.CreateInstance(\"100\"): {int64Model.CreateInstance("100")}");
        Console.WriteLine($"  BooleanModel.CreateInstance(): {booleanModel.CreateInstance()}");
        Console.WriteLine($"  BooleanModel.CreateInstance(\"true\"): {booleanModel.CreateInstance("true")}");

        // ============================================
        // 错误处理示例
        // ============================================

        Console.WriteLine("\n6. 错误处理示例:");
        Console.WriteLine("----------------------------------------");

        try
        {
            // User 只有一个参数构造函数，尝试使用0个参数应该抛出异常
            userModel.CreateInstance();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  参数数量错误: {ex.Message}");
        }

        try
        {
            // 尝试使用不匹配的参数数量
            userModel.CreateInstance(1, 2, 3);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  参数数量错误: {ex.Message}");
        }

        Console.WriteLine("\n============================================");
        Console.WriteLine("CreateInstance 功能演示完成");
        Console.WriteLine("============================================");
    }
}