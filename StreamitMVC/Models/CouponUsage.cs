using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class CouponUsage:BaseEntity
    {
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.Now;
    }
}
