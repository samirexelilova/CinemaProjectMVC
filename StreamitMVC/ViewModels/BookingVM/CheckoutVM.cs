using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels.BookingVM
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int ShowtimeId { get; set; }

        [Required(ErrorMessage = "Please select seats")]
        public string SelectedSeats { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [CreditCard(ErrorMessage = "Invalid credit card number")]
        [Display(Name = "Credit Card Number")]
        public string CreditCardNumber { get; set; }

        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Card Expiry Date")]
        public DateTime? CardExpiryDate { get; set; }
        [Display(Name = "Additional Notes")]
        public string OrderNotes { get; set; }
    }
}
