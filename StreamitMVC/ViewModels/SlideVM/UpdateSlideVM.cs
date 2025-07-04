using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateSlideVM
    {
        [MaxLength(50, ErrorMessage = "Title 50 simvoldan az olmalidir")]
        public string Name { get; set; }
        public IFormFile? Photo { get; set; }
        public IFormFile? TrailerVideoURL { get; set; }
        public string Image { get; set; }
        public string TrailerVideo { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal ImdbRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }
}
