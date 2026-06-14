using Prototype.Models;
using Prototype.Services;

namespace Prototype.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db, PasswordHasherService hasher)
        {
            if (!db.Users.Any())
            {
                db.Users.AddRange(
                    new User { Login = "employee", PasswordHash = hasher.Hash("12345"), Role = UserRole.Employee },
                    new User { Login = "manager", PasswordHash = hasher.Hash("12345"), Role = UserRole.Manager },
                    new User { Login = "lead", PasswordHash = hasher.Hash("12345"), Role = UserRole.Lead },
                    new User { Login = "sysadmin", PasswordHash = hasher.Hash("12345"), Role = UserRole.SysAdmin }
                );
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
