using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Email formatı düzgün deyil")]
        public string Email { get; set; }
    }
}
