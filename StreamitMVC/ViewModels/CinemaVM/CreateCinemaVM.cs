using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateCinemaVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public IFormFile Photo { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        [Phone]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "Open Time")]
        public TimeSpan OpenTime { get; set; }

        [Required]
        [Display(Name = "Close Time")]
        public TimeSpan CloseTime { get; set; }
    }
}
