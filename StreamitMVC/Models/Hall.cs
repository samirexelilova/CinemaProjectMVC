using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Hall:BaseEntity
    {
        public string Name { get; set; } 
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; }
        public int Capacity { get; set; }
        public int Rows { get; set; }  // Sıra sayı
        public int SeatsPerRow { get; set; }  // Hər sırada oturacaq sayı
        public List<Session> Sessions { get; set; }
        public List<Seat> Seats { get; set; }
        public int HallTypeId { get; set; }
        public HallType HallType { get; set; }
    }
}
