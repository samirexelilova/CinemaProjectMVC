using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class SeatIndexVM
    {
            public Hall Hall { get; set; }
            public List<Seat> Seats { get; set; }
            public List<SeatType> SeatTypes { get; set; }
    }
}
