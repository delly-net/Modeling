using Deme;

var user = new User(Guid.NewGuid().ToString("N"));
var model = User.GetModel();
Console.WriteLine(model.GetProperty(user, nameof(User.Id)));
model.SetProperty(user, nameof(User.Name), "model");
Console.WriteLine(model.GetProperty(user, nameof(User.Name)));