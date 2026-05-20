using Deme;
using System.Diagnostics;

var user = new User(Guid.NewGuid().ToString("N"));

var watch01 = new Stopwatch();
var watch02 = new Stopwatch();

// 反射方式
watch01.Start();
var type = typeof(User);
var property = type.GetProperty(nameof(User.Age));
for (long i = 0; i < 10000000; i++)
{
    property?.SetValue(user, i);
}
watch01.Stop();

// 源生成方式
watch02.Start();
var model = User.GetModel();
for (long i = 0; i < 10000000; i++)
{
    model.SetProperty(user, nameof(User.Age), i);
}
watch02.Stop();

Console.WriteLine($"{watch01.ElapsedMilliseconds}/{watch02.ElapsedMilliseconds}");