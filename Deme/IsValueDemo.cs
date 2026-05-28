using Delly.Modeling;
using Delly.Modeling.Models;
using Demo;

namespace Demo;

/// <summary>
/// IsValue 属性功能演示
/// </summary>
public class IsValueDemo
{
    /// <summary>
    /// 运行演示
    /// </summary>
    public void Run()
    {
        Console.WriteLine("============================================");
        Console.WriteLine("IsValue 属性演示");
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

        Console.WriteLine("\n============================================");
        Console.WriteLine("IsValue 属性演示完成");
        Console.WriteLine("============================================");
    }
}
