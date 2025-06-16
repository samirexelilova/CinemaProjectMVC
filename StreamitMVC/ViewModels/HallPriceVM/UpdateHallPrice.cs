using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class UpdateHallPrice
    {
        public int HallTypeId { get; set; }
        public decimal Price { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }

        public List<HallType>? HallTypes { get; set; }
    }
}
