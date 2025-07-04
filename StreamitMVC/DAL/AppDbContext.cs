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
        public DbSet<MovieStats> MovieStats { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
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
        public DbSet<MovieLanguage> MovieLanguages { get; set; }
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
        public DbSet<Slide> Slides { get; set; }
        public DbSet<ReviewReaction> ReviewReactions { get; set; }
        public DbSet<MoviePurchase> MoviePurchases { get; set; }
        public DbSet<MovieViewHistory> MovieViewHistories { get; set; }
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

            modelBuilder.Entity<Subtitle>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Subtitles)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
     .HasOne(s => s.Subtitle)
     .WithMany()
     .HasForeignKey(s => s.SubtitleId)
     .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReviewReaction>()
         .HasOne(r => r.Review)
         .WithMany()
         .HasForeignKey(r => r.ReviewId)
         .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReviewReaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
           .HasMany(b => b.Payments)  
           .WithOne(p => p.Booking)   
           .HasForeignKey(p => p.BookingId)
           .IsRequired(false) 
           .OnDelete(DeleteBehavior.Cascade); 


            modelBuilder.Entity<MoviePurchase>()
      .HasOne(mp => mp.Payment)
      .WithOne(p => p.MoviePurchase)
      .HasForeignKey<Payment>(p => p.MoviePurchaseId);


            modelBuilder.Entity<MovieViewHistory>()
       .HasOne(m => m.Movie)
       .WithMany()
       .HasForeignKey(m => m.MovieId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovieViewHistory>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovieViewHistory>()
                .HasOne(m => m.MoviePurchase)
                .WithMany()
                .HasForeignKey(m => m.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cinema>()
       .HasMany(c => c.Halls)
       .WithOne(h => h.Cinema)
       .HasForeignKey(h => h.CinemaId)
       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hall>()
                .HasMany(h => h.Seats)
                .WithOne(s => s.Hall)
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seat>()
                .HasMany(s => s.Tickets)
                .WithOne(t => t.Seat)
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Hall>()
    .HasMany(h => h.Sessions)
    .WithOne(s => s.Hall)
    .HasForeignKey(s => s.HallId)
    .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
