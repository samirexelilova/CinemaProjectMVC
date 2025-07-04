using StreamitMVC.Models.Base;
using StreamitMVC.Utilities.Enums;

namespace StreamitMVC.Models
{
    public class BasketItem : BaseEntity
    {
        public int BasketId { get; set; }
        public Basket Basket { get; set; }
        public int? SessionId { get; set; }
        public Session Session { get; set; }
        public int? SeatId { get; set; }
        public Seat Seat { get; set; }
        public decimal Price { get; set; }


        public int? MovieId { get; set; } 
        public Movie Movie { get; set; }
        public BasketItemType Type { get; set; } = BasketItemType.SessionTicket;
    }
}
