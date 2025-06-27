using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class SelectSeatsVM
    {
        public int SessionId { get; set; }
        public string HallName { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public Movie Movie { get; set; }
        public Session Session { get; set; }
        public List<SeatViewModel> Seats { get; set; }
        public string ErrorMessage { get; set; }
        public int MaxSeatsPerBooking { get; set; } = 6;

    }
}
