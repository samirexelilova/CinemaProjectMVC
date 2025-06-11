using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Refund:BaseEntity
    {
        public int BookingId { get; set; }   //Bu geri ödəmə (refund) hansı Booking-ə (sifarişə) aid olduğunu göstərir.
        public Booking Booking { get; set; }
        public int? PaymentId { get; set; }  // optional, çünki refund olmaya da bilər
        public Payment? Payment { get; set; }
        public decimal Amount { get; set; }  // Geri qaytarılan məbləğ
        public RefundStatus Status { get; set; } = RefundStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.Now;  //geri odeme ne zaman bas verib 
        public DateTime? ProcessedAt { get; set; }  //Geri ödəmə təsdiqlənib və ya rədd edilərək tamamlandığı tarix.
        public string Reason { get; set; } 
    }
}
