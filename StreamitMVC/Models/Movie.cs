using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Movie:BaseEntity
    {
        public string Name { get; set; }
        public string Photo { get; set; }
        public string TrailerVideo { get; set; }
        public string VideoUrl { get; set; }  // Bu film faylının URL-i olacaq
        public TimeSpan Duration { get; set; }
        public string Director { get; set; }
        public string Country { get; set; }
        public decimal ImdbRating { get; set; }
        public string Description { get; set; }
        public int AgeRestriction { get; set; }
        public DateTime ReleaseDate { get; set; } //filmin ne zaman cixdigi tarix
        public int LanguageId { get; set; }
        public Language Language { get; set; }  // Filmin dili
        public List<Subtitle> Subtitles { get; set; }  // Filmin altyazıları
        public List<Session> Sessions { get; set; }  // Filmin seansları
        public List<MovieActor> MovieActors { get; set; }
        public List<MovieTag> MovieTags { get; set; }
        public List<MovieCategory> MovieCategories { get; set; }
        public List<Review> Reviews { get; set; }

    }
}
