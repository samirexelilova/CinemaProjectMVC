using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class CreateSessionVM
    {
        public Session Session { get; set; }

        public List<Movie> Movies { get; set; }
        public List<Hall> Halls { get; set; }
        public List<Cinema> Cinemas { get; set; }
        public List<HallPrice> HallPrices { get; set; }
        public List<Language> Languages { get; set; }
    }
}
