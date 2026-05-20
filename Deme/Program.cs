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