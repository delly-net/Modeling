using Delly.Modeling;
using Demo;

var user = new User(Guid.NewGuid().ToString("N"));
var model = User.GetModel();
var propertyId = model.GetProperty(nameof(User.Id));
var propertyName = model.GetProperty(nameof(User.Name));
Console.WriteLine(propertyId?.GetValue(user));
propertyName?.SetValue(user, "model");
Console.WriteLine(propertyName?.GetValue(user));

var userEntity = new UserEntity(Guid.NewGuid().ToString("N"));
var entityModel = UserEntity.GetEntityModel();
foreach (var property in entityModel.GetProperties())
{
    Console.WriteLine($"{property.Name},{property.Comment}");
}

var userQueryModel = UserQuery.GetEntityModel();
Console.WriteLine($"\nUserQuery 模型信息:");
Console.WriteLine($"  表名: {userQueryModel.Name}");
Console.WriteLine($"  类名: {userQueryModel.ClassName}");
Console.WriteLine($"  命名空间: {userQueryModel.Namespace}");
Console.WriteLine($"  属性数量: {userQueryModel.GetProperties().Length}");

Console.WriteLine("\nUserQuery 属性列表:");
foreach (var property in userQueryModel.GetProperties())
{
    Console.WriteLine($"  {property.Name} ({property.Type}): {property.PropertyName}");
}

// ============================================
// 建模工厂示例
// ============================================

Console.WriteLine("\n============================================");
Console.WriteLine("建模工厂示例");
Console.WriteLine("============================================\n");

// 创建工厂实例
var factory = new DefaultEntityModelFactory();

// 方式1：直接添加单个模型
factory.Add(UserEntity.GetEntityModel());
factory.Add(UserQuery.GetEntityModel());

Console.WriteLine($"已注册模型总数: {factory.Count}");

// 方式2：使用建模集合添加（需要先创建建模集合类）
// [MoSet]
// public partial class EntityModelSet;
// var entitySet = new EntityModelSet();
// factory.AddSet(entitySet);

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

// 模块化开发示例
Console.WriteLine("\n============================================");
Console.WriteLine("模块化开发示例");
Console.WriteLine("============================================\n");

var moduleFactory = new DefaultEntityModelFactory();

// 模块A注册
var moduleA = new ModuleA(moduleFactory);
moduleA.RegisterModels();

Console.WriteLine($"模块A注册后模型数: {moduleFactory.Count}");

// 模块B注册
var moduleB = new ModuleB(moduleFactory);
moduleB.RegisterModels();

Console.WriteLine($"模块B注册后模型数: {moduleFactory.Count}");

Console.WriteLine("\n所有已注册模型:");
foreach (var m in moduleFactory.GetAllModels())
{
    Console.WriteLine($"  {ModelUtils.GetFullName(m)}");
}

// ============================================
// 模块类定义（示例）
// ============================================

/// <summary>
/// 模块A
/// </summary>
public class ModuleA
{
    private readonly DefaultEntityModelFactory _factory;

    public ModuleA(DefaultEntityModelFactory factory)
    {
        _factory = factory;
    }

    public void RegisterModels()
    {
        _factory.Add(UserEntity.GetEntityModel());
        _factory.Add(UserQuery.GetEntityModel());
    }
}

/// <summary>
/// 模块B
/// </summary>
public class ModuleB
{
    private readonly DefaultEntityModelFactory _factory;

    public ModuleB(DefaultEntityModelFactory factory)
    {
        _factory = factory;
    }

    public void RegisterModels()
    {
        // 这里可以注册其他模块的模型
    }
}