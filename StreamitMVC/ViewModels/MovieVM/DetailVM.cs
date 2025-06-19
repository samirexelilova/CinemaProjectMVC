using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.ViewModels
{
    public class DetailVM
    {
        public Movie Movie { get; set; }
        public List<Movie> RelatedMovies { get; set; }
        public List<SessionWithPriceViewModel> SessionPrices { get; set; }


    }
}
