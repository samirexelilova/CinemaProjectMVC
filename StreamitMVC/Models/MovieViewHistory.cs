using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class MovieViewHistory:BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public DateTime ViewDate { get; set; }

        public int? StoppedAtMinute { get; set; } 

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [ForeignKey("MovieId")]
        public Movie Movie { get; set; }

        [ForeignKey("PurchaseId")]
        public MoviePurchase MoviePurchase { get; set; }
    }
}
