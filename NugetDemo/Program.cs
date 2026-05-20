using Deme;
using System.Diagnostics;

var user = new User(Guid.NewGuid().ToString("N"));

var watch01 = new Stopwatch();
var watch02 = new Stopwatch();
var watch03 = new Stopwatch();
var watch04 = new Stopwatch();

{
    //// 反射方式
    //watch03.Start();
    //var type = typeof(User);
    //for (long i = 0; i < 10000000; i++)
    //{
    //    var property = type.GetProperty(nameof(User.Age));
    //    property?.SetValue(user, i);
    //}
    //watch03.Stop();

    // 源生成方式
    watch04.Start();
    var model = User.GetModel();
    for (long i = 0; i < 10000000; i++)
    {
        var propertyAge = model.GetProperty(nameof(User.Age));
        propertyAge?.SetValue(user, i);
    }
    watch04.Stop();
    var eff01 = 1.0 / 1 + watch03.ElapsedMilliseconds;
    var eff02 = 1.0 / watch04.ElapsedMilliseconds;
    var percent = eff02 / eff01 * 100;
    Console.WriteLine("即时赋值算法，1000万运算测试");
    Console.WriteLine($"Ref: {watch03.ElapsedMilliseconds}ms, Gen: {watch04.ElapsedMilliseconds}ms, up {percent:f2}%");
}

{
    // 反射方式
    watch01.Start();
    //var type = typeof(User);
    //var property = type.GetProperty(nameof(User.Age));
    //for (long i = 0; i < 10000000; i++)
    //{
    //    property?.SetValue(user, i);
    //}
    watch01.Stop();

    // 源生成方式
    watch02.Start();
    var model = User.GetModel();
    var propertyAge = model.GetProperty(nameof(User.Age));
    for (long i = 0; i < 10000000; i++)
    {
        propertyAge?.SetValue(user, i);
    }
    watch02.Stop();

    var eff01 = 1.0 / 1 + watch01.ElapsedMilliseconds;
    var eff02 = 1.0 / watch02.ElapsedMilliseconds;
    var percent = eff02 / eff01 * 100;
    Console.WriteLine("最优赋值算法，1000万运算测试");
    Console.WriteLine($"Ref: {watch01.ElapsedMilliseconds}ms, Gen: {watch02.ElapsedMilliseconds}ms, up {percent:f2}%");
}

Console.WriteLine();
