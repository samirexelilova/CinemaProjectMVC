using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Seat : BaseEntity
    {
        public int HallId { get; set; }
        public Hall Hall { get; set; }

        public int RowNumber { get; set; }    
        public int SeatNumber { get; set; }    

    }
}
