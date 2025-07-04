using StreamitMVC.Models.Base;
using StreamitMVC.Utilities.Enums;

namespace StreamitMVC.Models
{
    public class MoviePurchase:BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public DateTime? ExpiresAt { get; set; } // Məsələn 30 gün
        public MoviePurchaseStatus Status { get; set; } = MoviePurchaseStatus.Active;
        public Payment Payment { get; set; }
        public int? PaymentId { get; set; }
    }
}
