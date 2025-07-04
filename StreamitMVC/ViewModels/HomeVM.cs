using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class HomeVM
    {
        public Movie Movie { get; set; }
        public List<Movie> Movies { get; set; }
        public List<Movie> UpcomingMovies { get; set; }
        public List<Movie> LatestMovies { get; set; }
        public List<Movie> SuggestedMovies { get; set; }
        public Movie TopMovieOfYear { get; set; }
    }
}
