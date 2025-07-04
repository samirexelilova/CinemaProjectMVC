using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Movie:BaseEntity
    {
        public string Name { get; set; }
        public string Photo { get; set; }
        public string TrailerVideo { get; set; }


        public string VideoUrl { get; set; }  
        public decimal? DirectPurchasePrice { get; set; } // Filmi birbaşa almaq üçün qiymət
        public bool IsAvailableForDirectPurchase { get; set; } = false;
        public List<MoviePurchase> MoviePurchases { get; set; }




        public TimeSpan Duration { get; set; }
        public string Director { get; set; }
        public string Country { get; set; }
        public decimal ImdbRating { get; set; }
        public string Description { get; set; }
        public int AgeRestriction { get; set; }
        public DateTime ReleaseDate { get; set; } //filmin ne zaman cixdigi tarix
        public List<Subtitle> Subtitles { get; set; }  // Filmin altyazıları
        public List<Session> Sessions { get; set; }  // Filmin seansları
        public List<MovieActor> MovieActors { get; set; }
        public List<MovieTag> MovieTags { get; set; }
        public List<MovieCategory> MovieCategories { get; set; }
        public List<MovieLanguage> MovieLanguages { get; set; }
        public List<Review> Reviews { get; set; }
        public MovieStats MovieStats { get; set; }
    }
}
