using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class SeatType:BaseEntity
    {
        public string Name { get; set; }  // "Empty", "Reserved" və s.
        public string Color { get; set; } // Məsələn: "#00FF00"
    }
}
