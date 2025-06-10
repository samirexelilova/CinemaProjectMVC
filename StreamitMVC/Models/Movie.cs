using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Movie:BaseEntity
    {
        public string Name { get; set; }
        public string Photo { get; set; }
        public string TrailerVideo { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; } //filmin ne zaman cixdigi tarix
        public int LanguageId { get; set; }
        public Language Language { get; set; }  // Filmin dili
        public List<Subtitle> Subtitles { get; set; }  // Filmin altyazıları
        public List<Session> Sessions { get; set; }  // Filmin seansları
        public List<MovieActor> MovieActors { get; set; }
        public List<MovieTag> MovieTags { get; set; }

    }
}
