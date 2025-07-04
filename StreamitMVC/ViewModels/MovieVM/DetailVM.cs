using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.ViewModels
{
    public class DetailVM
    {
        public Movie Movie { get; set; }
        public List<Movie> RelatedMovies { get; set; }
        public List<SessionWithPriceViewModel> SessionPrices { get; set; }
        public List<Favorite> Favorites { get; set; }
        public bool IsMovieFavorited { get; set; }
        public int? SelectedLanguageId { get; set; }
        public int? SelectedCinemaId { get; set; }
        public DateTime? SelectedDate { get; set; }
        public List<Language> Languages { get; set; }
        public List<Cinema> Cinemas
        {
            get; set;
        }
    }
}
