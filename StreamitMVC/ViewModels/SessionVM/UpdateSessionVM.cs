using Microsoft.AspNetCore.Mvc.Rendering;
using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateSessionVM
    {

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [Required]
        public int HallId { get; set; }

        [Required]
        public int HallPriceId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        public int? SubtitleId { get; set; }

        public List<Movie>? Movies { get; set; }
        public List<Cinema>? Cinemas { get; set; }
        public List<Hall>? Halls { get; set; }
        public List<HallPrice>? HallPrices { get; set; }
        public List<Language>? Languages { get; set; }
        public List<Subtitle>? Subtitles { get; set; }

    }
}
