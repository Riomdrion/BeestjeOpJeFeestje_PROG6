using BeestjeOpJeFeestje_PROG6.data.Models;
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
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.adress).IsRequired();
                entity.Property(e => e.Card)
                    .HasMaxLength(10)
                    .IsRequired(false);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventDate).IsRequired();
                entity.Property(e => e.IsConfirmed).IsRequired();
                entity.Property(e => e.discount).IsRequired();
                entity.Property(e => e.price).IsRequired().HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Animals)
                    .WithMany(a => a.Bookings)
                    .UsingEntity<Dictionary<string, object>>(
                        "BookingAnimal",
                        j => j.HasOne<Animal>().WithMany().HasForeignKey("AnimalId"),
                        j => j.HasOne<Booking>().WithMany().HasForeignKey("BookingId"));
            });
        }
    }
}