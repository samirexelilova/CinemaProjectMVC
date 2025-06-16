using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateCinemaVM
    {
        [Required]
        public string Name { get; set; }

        public IFormFile? Photo { get; set; }
        public string Image { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "Open Time")]
        public TimeSpan OpenTime { get; set; }

        [Required]
        [Display(Name = "Close Time")]
        public TimeSpan CloseTime { get; set; }
    }
}
