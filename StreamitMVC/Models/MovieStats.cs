using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class MovieStats:BaseEntity
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int ViewCount { get; set; }
    }
}
