using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeestjeOpJeFeestje_PROG6.data.SeedData
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Check if the database is already seeded
                if (context.Animals.Any() || context.Users.Any() || context.Bookings.Any())
                {
                    return; // DB has been seeded
                }

                // Voeg eerst Animals en Users toe
                var animals = new[]
                {
                    new Animal { Name = "Aap", Type = "Jungle", Price = 50.00, ImageUrl = "https://example.com/aap.jpg" },
                    new Animal { Name = "Koe", Type = "Boerderij", Price = 75.00, ImageUrl = "https://example.com/koe.jpg" },
                    new Animal { Name = "Pinguïn", Type = "Sneeuw", Price = 100.00, ImageUrl = "https://example.com/pinguin.jpg" },
                    new Animal { Name = "Kameel", Type = "Woestijn", Price = 85.00, ImageUrl = "https://example.com/kameel.jpg" },
                    new Animal { Name = "Unicorn", Type = "VIP", Price = 500.00, ImageUrl = "https://example.com/unicorn.jpg" }
                };
                context.Animals.AddRange(animals);
                context.SaveChanges();

                var users = new[]
                {
                    new User { Email = "Admin@example.com", PasswordHash = "hashed_password1", PhoneNumber = 0643289141, Role = 1},
                    new User { Email = "Silver@example.com", PasswordHash = "hashed_password2", PhoneNumber = 0642259141, Role = 0, Card = "Silver"},
                    new User { Email = "Gold@example.com", PasswordHash = "hashed_password3", PhoneNumber = 0645289541, Role = 0, Card = "Gold"},
                    new User { Email = "Platinum4@example.com", PasswordHash = "hashed_password4" , PhoneNumber = 0642269541, Role = 0, Card = "Platinum" },
                    new User { Email = "User@example.com", PasswordHash = "hashed_password5" , PhoneNumber = 0643189341, Role = 0}
                };
                context.Users.AddRange(users);
                context.SaveChanges();

                context.Bookings.AddRange(
                    new Booking { UserId = users[0].Id, AnimalId = animals[0].Id, EventDate = DateTime.Now.AddDays(1) },
                    new Booking { UserId = users[1].Id, AnimalId = animals[1].Id, EventDate = DateTime.Now.AddDays(2) },
                    new Booking { UserId = users[2].Id, AnimalId = animals[2].Id, EventDate = DateTime.Now.AddDays(3) },
                    new Booking { UserId = users[3].Id, AnimalId = animals[3].Id, EventDate = DateTime.Now.AddDays(4) },
                    new Booking { UserId = users[4].Id, AnimalId = animals[4].Id, EventDate = DateTime.Now.AddDays(5) }
                );
                context.SaveChanges();


            }
        }
    }
}
