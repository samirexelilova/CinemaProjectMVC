using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class MovieVM
    {
        public List<Movie> Movies { get; set; }
        public List<Language> Languages { get; set; }
        public List<Cinema> Cinemas { get; set; }
        public List<Slide> Slides { get; set; }

        public int? SelectedLanguageId { get; set; }
        public int? SelectedCinemaId { get; set; }
        public DateTime? SelectedDate { get; set; }
    }
}
