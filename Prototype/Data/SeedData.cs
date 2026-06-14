using Prototype.Models;
using Prototype.Services;

namespace Prototype.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db, PasswordHasherService hasher)
        {
            var defaultUsers = new (string Login, string Password, UserRole Role)[]
            {
                ("employee", "12345", UserRole.Employee),
                ("manager", "12345", UserRole.Manager),
                ("lead", "12345", UserRole.Lead),
                ("sysadmin", "12345", UserRole.SysAdmin)
            };

            if (!db.Users.Any())
            {
                foreach (var defaultUser in defaultUsers)
                {
                    var user = new User
                    {
                        Login = defaultUser.Login,
                        Role = defaultUser.Role
                    };

                    user.PasswordHash = hasher.Hash(user, defaultUser.Password);
                    db.Users.Add(user);
                }
            }
            else
            {
                foreach (var defaultUser in defaultUsers)
                {
                    var user = db.Users.FirstOrDefault(x => x.Login == defaultUser.Login);
                    if (user is not null && !hasher.Verify(user, defaultUser.Password))
                    {
                        user.PasswordHash = hasher.Hash(user, defaultUser.Password);
                    }
                }
            }

            if (!db.Products.Any())
            {
                db.Products.AddRange(
                    new Product { Name = "Сканер штрихкодов", Description = "2D Scanner", QuantityInStock = 20 },
                    new Product { Name = "Принтер этикеток", Description = "Label printer", QuantityInStock = 10 },
                    new Product { Name = "Термобумага", Description = "Рулон 58 мм", QuantityInStock = 150 }
                );
            }

            db.SaveChanges();

            if (!db.Orders.Any())
            {
                var firstProduct = db.Products.First();
                var secondProduct = db.Products.Skip(1).First();

                var order = new Order
                {
                    OrderNumber = "PVZ-10001",
                    Status = OrderStatus.Created,
                    Items =
                    [
                        new OrderItem { ProductId = firstProduct.Id, Quantity = 2 },
                    new OrderItem { ProductId = secondProduct.Id, Quantity = 1 }
                    ]
                };

                db.Orders.Add(order);
                db.SaveChanges();
            }
        }
    }
}
