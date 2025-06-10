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

        public List<Session> Sessions { get; set; }
        public HallType Type { get; set; }
    }
}
