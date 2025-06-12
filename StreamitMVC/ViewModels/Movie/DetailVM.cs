using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class DetailVM
    {
        public Movie Movie { get; set; }
        public List<Movie> RelatedMovies { get; set; }

    }
}
