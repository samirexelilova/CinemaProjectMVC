using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateReviewVM
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public int MovieId { get; set; }
    }
}
