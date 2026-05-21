using Microsoft.EntityFrameworkCore;
using backend.Models; 

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<backend.Models.Route> Routes { get; set; }
        public DbSet<RoutePoint> RoutePoints { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SupplyList> SupplyLists { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>()
                .Property(reservation => reservation.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(payment => payment.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Trip>()
                .HasOne(trip => trip.Route)
                .WithMany()
                .HasForeignKey(trip => trip.RouteId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
