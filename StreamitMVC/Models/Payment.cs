using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Payment : BaseEntity //odenis melumatlari ucun 
    {

        public PaymentMethod Method { get; set; } //ödəmə metodunu
        public decimal Amount { get; set; }  //umumi mebleg 
        public DateTime PaidAt { get; set; } = DateTime.Now; //Odənişin tarixi və saat
        public int? BookingId { get; set; } //Yəni bu odenis hansı bilet bron edilməsinə aiddirsə onu gostərir
        public Booking Booking { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? FailureReason { get; set; }  // Uğursuzluq səbəbi

        public int? MoviePurchaseId { get; set; }
        public MoviePurchase MoviePurchase { get; set; }

    }
}
