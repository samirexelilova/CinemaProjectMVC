using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class HallPrice : BaseEntity
    {
        public HallType HallType { get; set; }
        public decimal Price { get; set; }
    }
}
