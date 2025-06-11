using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class MoviesByDateViewModel
    {

        public DateTime SelectedDate { get; set; }
        public List<Movie> Movies { get; set; }
    }
}
