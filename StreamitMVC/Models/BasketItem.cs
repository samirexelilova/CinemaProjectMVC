using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class BasketItem : BaseEntity
    {
        public int BasketId { get; set; }
        public Basket Basket { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; }
        public int SeatId { get; set; }
        public Seat Seat { get; set; }
        public decimal Price { get; set; }
    }
}
