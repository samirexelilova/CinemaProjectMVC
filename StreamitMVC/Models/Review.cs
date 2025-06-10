using StreamitMVC.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.Models
{
    public class Review:BaseEntity
    {
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
