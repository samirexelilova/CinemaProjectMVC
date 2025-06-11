using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; } // məs: 10 => 10%
        public decimal? MinAmount { get; set; }  // Minimum məbləğ
        public decimal? MaxDiscount { get; set; }  // Maksimum endirim
        public bool IsActive { get; set; } = true;
        public DateTime ExpireDate { get; set; } //bitme tarixi
        public int UsageLimit { get; set; } //ne qeder istifade oluna biler
        public int TimesUsed { get; set; }//ne qederi istifade olunub artiq 
        public List<CouponUsage> CouponUsages { get; set; }
    }
}
