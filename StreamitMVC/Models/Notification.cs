using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;  // Bildiriş oxunub-oxunmadığını göstərir
        public DateTime? DeliveredAt { get; set; }  // Nə vaxt çatdırılıb?
    }
}
