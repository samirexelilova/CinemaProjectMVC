using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Session:BaseEntity
    {
        public DateTime StartTime { get; set; }  
        public DateTime EndTime { get; set; } 

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int HallId { get; set; }
        public Hall Hall { get; set; }
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; }
        public int HallPriceId { get; set; }  // Zal tipinin qiyməti ilə əlaqə
        public HallPrice HallPrice { get; set; }
        public int LanguageId { get; set; }
        public Language Language { get; set; }
        public int AvailableSeats { get; set; }
        public List<Booking> Bookings { get; set; } 
        public List<Ticket> Tickets { get; set; } 
        public List<BasketItem> BasketItems { get; set; } 
    }
}
