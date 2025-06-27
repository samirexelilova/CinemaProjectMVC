using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Booking : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now; //sifarisin yarandigi vaxt

        public BookingStatus Status { get; set; }=BookingStatus.Reserved;
        public decimal TotalAmount { get; set; }  // Ümumi məbləğ
        public List<Ticket> Tickets { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

   

}
