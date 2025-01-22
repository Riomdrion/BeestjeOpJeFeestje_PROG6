using Microsoft.EntityFrameworkCore;

namespace BeestjeOpJeFeestje_PROG6.data.DBcontext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSet properties here
        public DbSet<Animal> Animals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Animal entity
            modelBuilder.Entity<Animal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImageUrl).HasMaxLength(250);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.AnimalId });
                entity.Property(e => e.EventDate).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Animal)
                    .WithMany(a => a.Bookings)
                    .HasForeignKey(e => e.AnimalId);
            });
        }
    }

    // Entity definitions
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }

    public class Booking
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int AnimalId { get; set; }
        public Animal Animal { get; set; }
        public DateTime EventDate { get; set; }
    }
}