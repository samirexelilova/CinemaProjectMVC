using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class WatchMovieViewModel
    {
        public Movie Movie { get; set; }
        public bool HasTicket { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool HasCinemaTicket { get; set; } 
        public bool HasHomeViewingTicket { get; set; }
    }
}
