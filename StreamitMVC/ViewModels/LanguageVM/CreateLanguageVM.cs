using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateLanguageVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }
        public IFormFile FlagImage { get; set; }
        public List<Movie>? AllMovies { get; set; }
    }
}
