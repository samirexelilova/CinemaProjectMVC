using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class HallPrice : BaseEntity
    {
        public int HallTypeId { get; set; }
        public HallType HallType { get; set; } 
        public decimal Price { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }  // Həftənin günü üçün
        public List<Session> Sessions { get; set; }
    }
}
