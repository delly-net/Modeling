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