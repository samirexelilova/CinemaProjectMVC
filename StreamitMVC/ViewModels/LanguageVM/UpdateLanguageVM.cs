using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateLanguageVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }
        public IFormFile? FlagImage { get; set; }
        public string ExistedFlagImage { get; set; }
        public List<Movie>? AllMovies { get; set; }

    }
}
