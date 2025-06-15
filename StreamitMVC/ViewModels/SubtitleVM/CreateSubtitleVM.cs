using Microsoft.AspNetCore.Mvc.Rendering;
using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateSubtitleVM
    {
        [Required]
        public int MovieId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [Required]
        public IFormFile SubtitleFile { get; set; }  

        public List<Movie>? AllMovies { get; set; }
        public List<Language>? AllLanguages { get; set; }
    }
}
