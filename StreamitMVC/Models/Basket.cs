using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Basket:BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public List<BasketItem> Items { get; set; }
        public decimal TotalPrice { get; set; }
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }
    }
}
