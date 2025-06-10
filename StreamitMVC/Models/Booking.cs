using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Booking : BaseEntity
    {
        public int UserId { get; set; }
        public AppUser User { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public BookingStatus Status { get; set; }=BookingStatus.Reserved;

        public List<Ticket> Tickets { get; set; } 
    }

   

}
