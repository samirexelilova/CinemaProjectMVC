using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.Models;

namespace StreamitMVC.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Actor> Actors { get; set; }
        public DbSet<ActorSocialMedia> ActorSocialMedias { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<HallPrice> HallPrices { get; set; }
        public DbSet<HallType> HallTypes { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<MovieCategory> MovieCategories { get; set; }
        public DbSet<MovieTag> MovieTags { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatType> SeatTypes { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Subtitle> Subtitles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Cinema)
                .WithMany(c => c.Sessions)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.HallPrice)
                .WithMany(hp => hp.Sessions)
                .HasForeignKey(s => s.HallPriceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Hall)
                .WithMany(h => h.Sessions)
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Language)
                .WithMany(l => l.Sessions)
                .HasForeignKey(s => s.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Sessions)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Subtitle>()
              .HasOne(s => s.Movie)
              .WithMany(m => m.Subtitles)
               .HasForeignKey(s => s.MovieId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Subtitle>()
                .HasOne(s => s.Language)
                .WithMany(l => l.Subtitles)
                .HasForeignKey(s => s.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);

             modelBuilder.Entity<Ticket>()
               .HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
              .HasForeignKey(t => t.BookingId)
             .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Session)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

        }


    }
}
