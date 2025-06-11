using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class HallType : BaseEntity
    {
        public string Name { get; set; }     // Standard, Comfort, SkyBox və s.
        public decimal ExtraCharge { get; set; }  // Əlavə qiymət fərqi üçün
        public List<Hall> Halls { get; set; }
        public List<HallPrice> HallPrices { get; set; }
    }
}
