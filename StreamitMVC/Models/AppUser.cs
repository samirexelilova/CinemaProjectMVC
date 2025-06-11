using Microsoft.AspNetCore.Identity;
using StreamitMVC.Extensions.Enums;

namespace StreamitMVC.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public Gender Gender { get; set; }
        public string Image { get; set; }
        public DateTime? BirthDate { get; set; }  // Əlavə edildi
        public string? Address { get; set; }  // Əlavə edildi

        public List<Basket> Baskets { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<Review> Reviews { get; set; }
        public List<CouponUsage> CouponUsages { get; set; }
        public List<Notification> Notifications { get; set; } 
    }
}
