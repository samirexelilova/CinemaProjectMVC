using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class GetMovieVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string TrailerVideo { get; set; }
        public string VideoUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public string Director { get; set; }
        public string Country { get; set; }
        public decimal ImdbRating { get; set; }
        public int AgeRestriction { get; set; }
        public DateTime ReleaseDate { get; set; }

        // List məlumatlar
        public List<Category> Categories { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Actor> Actors { get; set; }
        public List<Language> Languages { get; set; }
        public List<Language> Subtitles { get; set; }
        public int SessionCount { get; set; }
        public int ReviewCount { get; set; }

    }
}
