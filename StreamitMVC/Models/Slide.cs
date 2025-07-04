using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Slide:BaseEntity
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string TrailerVideo { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal ImdbRating { get; set; }
        public DateTime ReleaseDate { get; set; } //filmin ne zaman cixdigi tarix
        public string Description { get; set; }
        public int Order { get; set; }

    }
}
