using Delly.Modeling;
using Demo;

// ============================================
// 功能演示
// ============================================

var modelableDemo = new ModelableDemo();
modelableDemo.Run();

var moTableDemo = new MoTableDemo();
moTableDemo.Run();

var moQueryDemo = new MoQueryDemo();
moQueryDemo.Run();

var modelFactoryDemo = new ModelFactoryDemo();
modelFactoryDemo.Run();

var modelSetDemo = new ModelSetDemo();
modelSetDemo.Run();

var createInstanceDemo = new CreateInstanceDemo();
createInstanceDemo.Run();

var isValueDemo = new IsValueDemo();
isValueDemo.Run();

var baseTypeModelDemo = new BaseTypeModelDemo();
baseTypeModelDemo.Run();

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
