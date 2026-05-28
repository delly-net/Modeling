using Delly.Modeling;
using Demo;

namespace Demo;

/// <summary>
/// 基础类型建模工厂演示
/// </summary>
public class BaseTypeModelDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("基础类型建模工厂演示");
        Console.WriteLine("============================================\n");

        // 创建工厂实例（自动注册所有基础类型建模）
        var factory = new DefaultEntityModelFactory();

        Console.WriteLine($"已注册模型总数: {factory.Count}");

        // 按类全称查询
        Console.WriteLine("\n按类全称查询基础类型建模:");
        var stringModel = factory.GetModel("System.String");
        if (stringModel != null)
        {
            Console.WriteLine($"  System.String: {stringModel.ClassName}, IsValue: {stringModel.IsValue}");
        }

        var intModel = factory.GetModel("System.Int32");
        if (intModel != null)
        {
            Console.WriteLine($"  System.Int32: {intModel.ClassName}, IsValue: {intModel.IsValue}");
        }

        // 使用泛型方法查询（支持所有类型，包括值类型）
        Console.WriteLine("\n使用泛型方法查询基础类型建模:");
        var stringModel2 = factory.GetModel<string>();
        if (stringModel2 != null)
        {
            Console.WriteLine($"  GetModel<string>: {stringModel2.ClassName}, IsValue: {stringModel2.IsValue}");
        }

        // 值类型泛型查询
        var intModelGeneric = factory.GetModel<int>();
        Console.WriteLine($"  GetModel<int>: {intModelGeneric.ClassName}, IsValue: {intModelGeneric.IsValue}");

        var longModelGeneric = factory.GetModel<long>();
        Console.WriteLine($"  GetModel<long>: {longModelGeneric.ClassName}, IsValue: {longModelGeneric.IsValue}");

        var boolModelGeneric = factory.GetModel<bool>();
        Console.WriteLine($"  GetModel<bool>: {boolModelGeneric.ClassName}, IsValue: {boolModelGeneric.IsValue}");

        var doubleModelGeneric = factory.GetModel<double>();
        Console.WriteLine($"  GetModel<double>: {doubleModelGeneric.ClassName}, IsValue: {doubleModelGeneric.IsValue}");

        var decimalModelGeneric = factory.GetModel<decimal>();
        Console.WriteLine($"  GetModel<decimal>: {decimalModelGeneric.ClassName}, IsValue: {decimalModelGeneric.IsValue}");

        var dateTimeModelGeneric = factory.GetModel<DateTime>();
        Console.WriteLine($"  GetModel<DateTime>: {dateTimeModelGeneric.ClassName}, IsValue: {dateTimeModelGeneric.IsValue}");

        var guidModelGeneric = factory.GetModel<Guid>();
        Console.WriteLine($"  GetModel<Guid>: {guidModelGeneric.ClassName}, IsValue: {guidModelGeneric.IsValue}");

        // 值类型通过类全称查询
        Console.WriteLine("\n使用类全称查询值类型建模:");
        var intModel2 = factory.GetModel("System.Int32");
        if (intModel2 != null)
        {
            Console.WriteLine($"  GetModel(\"System.Int32\"): {intModel2.ClassName}, IsValue: {intModel2.IsValue}");
        }

        var longModel = factory.GetModel("System.Int64");
        if (longModel != null)
        {
            Console.WriteLine($"  GetModel(\"System.Int64\"): {longModel.ClassName}, IsValue: {longModel.IsValue}");
        }

        var boolModel = factory.GetModel("System.Boolean");
        if (boolModel != null)
        {
            Console.WriteLine($"  GetModel(\"System.Boolean\"): {boolModel.ClassName}, IsValue: {boolModel.IsValue}");
        }

        var doubleModel = factory.GetModel("System.Double");
        if (doubleModel != null)
        {
            Console.WriteLine($"  GetModel(\"System.Double\"): {doubleModel.ClassName}, IsValue: {doubleModel.IsValue}");
        }

        var decimalModel = factory.GetModel("System.Decimal");
        if (decimalModel != null)
        {
            Console.WriteLine($"  GetModel(\"System.Decimal\"): {decimalModel.ClassName}, IsValue: {decimalModel.IsValue}");
        }

        var dateTimeModel = factory.GetModel("System.DateTime");
        if (dateTimeModel != null)
        {
            Console.WriteLine($"  GetModel(\"System.DateTime\"): {dateTimeModel.ClassName}, IsValue: {dateTimeModel.IsValue}");
        }

        var guidModel = factory.GetModel("System.Guid");
        if (guidModel != null)
        {
            Console.WriteLine($"  GetModel(\"System.Guid\"): {guidModel.ClassName}, IsValue: {guidModel.IsValue}");
        }

        // 使用基础类型建模的 Parse 功能
        Console.WriteLine("\n使用基础类型建模的 Parse 功能:");
        if (stringModel != null)
        {
            var parsed = stringModel.Parse(123);
            Console.WriteLine($"  StringModel.Parse(123): {parsed}");
        }

        if (intModel != null)
        {
            var parsed = intModel.Parse("456");
            Console.WriteLine($"  Int32Model.Parse(\"456\"): {parsed}");
        }

        // 基础类型建模的属性和索引为空
        Console.WriteLine("\n基础类型建模的属性和索引:");
        if (stringModel != null)
        {
            var properties = stringModel.GetProperties();
            var indexes = stringModel.GetIndexes();
            var property = stringModel.GetProperty("AnyName");
            Console.WriteLine($"  StringModel.GetProperties(): {properties.Length} 个属性");
            Console.WriteLine($"  StringModel.GetIndexes(): {indexes.Length} 个索引");
            Console.WriteLine($"  StringModel.GetProperty(\"AnyName\"): {(property == null ? "null" : "非 null")}");
        }

        // TryGetModel 测试
        Console.WriteLine("\nTryGetModel 测试:");
        var tryStringModel = factory.TryGetModel<string>();
        if (tryStringModel != null)
        {
            Console.WriteLine($"  TryGetModel<string>: {tryStringModel.ClassName}");
        }

        // 值类型泛型 TryGetModel
        var tryIntModel = factory.TryGetModel<int>();
        if (tryIntModel != null)
        {
            Console.WriteLine($"  TryGetModel<int>: {tryIntModel.ClassName}");
        }

        var tryBoolModel = factory.TryGetModel<bool>();
        if (tryBoolModel != null)
        {
            Console.WriteLine($"  TryGetModel<bool>: {tryBoolModel.ClassName}");
        }

        var notExistModel = factory.TryGetModel<BaseTypeModelDemo>();
        if (notExistModel == null)
        {
            Console.WriteLine($"  TryGetModel<BaseTypeModelDemo>: null（符合预期）");
        }

        var notExistModel2 = factory.GetModel("System.NotExist");
        if (notExistModel2 == null)
        {
            Console.WriteLine($"  GetModel(\"System.NotExist\"): null（符合预期）");
        }

        Console.WriteLine("\n============================================");
        Console.WriteLine("基础类型建模工厂演示完成");
        Console.WriteLine("============================================");
    }
}
