using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Cinema:BaseEntity
    {
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }

        // Açılış və bağlanış saatı
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        // Əlaqəli seanslar
        public List<Session> Sessions { get; set; }

        public List<Hall> Halls { get; set; }
    }
}
