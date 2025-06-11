using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.Models
{
    public class Review:BaseEntity
    {
        [Range(1, 5)]
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending; // Default false, sonra admin təsdiqləyər
        public string? RejectedComment { get; set; }
        public int LikeCount { get; set; } = 0;  // Bəyənmə sayı
        public int DislikeCount { get; set; } = 0;  // Bəyənməmə sayı
    }
}
