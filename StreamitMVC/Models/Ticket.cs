using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Ticket : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        public decimal Price { get; set; }

        public TicketStatus Status { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.Now;
    }

  

}
