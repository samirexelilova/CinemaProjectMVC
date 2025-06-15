using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateSubtitleVM
    {
        [Required]
        public int MovieId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        public string ExistedFilePath { get; set; }

        public IFormFile? SubtitleFile { get; set; } 
        public List<Movie>? AllMovies { get; set; }
        public List<Language>? AllLanguages { get; set; }
    }
}
