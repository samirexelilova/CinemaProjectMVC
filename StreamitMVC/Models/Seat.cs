using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Seat : BaseEntity
    {
        public int HallId { get; set; }
        public Hall Hall { get; set; }

        public int RowNumber { get; set; }    
        public int SeatNumber { get; set; }
        public int SeatTypeId { get; set; }  // Və ya HallTypeId
        public SeatType SeatType { get; set; }  // Əlavə class lazımdır
        public List<BasketItem> BasketItems { get; set; }
        public List<Ticket> Tickets { get; set; }
    }
}
