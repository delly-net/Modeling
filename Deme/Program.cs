using Delly.Modeling;
using Delly.Modeling.Models;
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

// ============================================
// 建模集合示例
// ============================================

Console.WriteLine("\n============================================");
Console.WriteLine("建模集合泛型方法示例");
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

// ============================================
// CreateInstance 功能演示
// ============================================

var demo = new CreateInstanceDemo();
demo.Run();

// ============================================
// IsValue 属性演示
// ============================================

Console.WriteLine("\n============================================");
Console.WriteLine("IsValue 属性示例");
Console.WriteLine("============================================\n");

// 基础值类型
Console.WriteLine("基础值类型 IsValue:");
Console.WriteLine($"  string.IsValue: {StringModel.Instance.IsValue}");  // true
Console.WriteLine($"  int.IsValue: {Int32Model.Instance.IsValue}");      // true
Console.WriteLine($"  long.IsValue: {Int64Model.Instance.IsValue}");     // true
Console.WriteLine($"  bool.IsValue: {BooleanModel.Instance.IsValue}");   // true
Console.WriteLine($"  double.IsValue: {DoubleModel.Instance.IsValue}");  // true
Console.WriteLine($"  decimal.IsValue: {DecimalModel.Instance.IsValue}"); // true
Console.WriteLine($"  DateTime.IsValue: {DateTimeModel.Instance.IsValue}"); // true
Console.WriteLine($"  Guid.IsValue: {GuidModel.Instance.IsValue}");       // true

// 一般 class 类型
Console.WriteLine("\n一般 class 类型 IsValue:");
Console.WriteLine($"  User.IsValue: {User.GetModel().IsValue}");          // false

// ============================================
// 建模集合类定义
// ============================================

/// <summary>
/// 实体模型集合
/// </summary>
[MoSet]
public partial class EntityModelSet;

// ============================================
// 模块类定义（示例）
// ============================================

/// <summary>
/// 模块A
/// </summary>
public class ModuleA
{
    private readonly DefaultEntityModelFactory _factory;

    /// <summary>
    /// 初始化 ModuleA 的新实例
    /// </summary>
    /// <param name="factory">实体模型工厂</param>
    public ModuleA(DefaultEntityModelFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// 注册模块A的模型
    /// </summary>
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

    /// <summary>
    /// 初始化 ModuleB 的新实例
    /// </summary>
    /// <param name="factory">实体模型工厂</param>
    public ModuleB(DefaultEntityModelFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// 注册模块B的模型
    /// </summary>
    public void RegisterModels()
    {
        // 这里可以注册其他模块的模型
    }
}