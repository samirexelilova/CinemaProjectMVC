using StreamitMVC.Extensions.Enums;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public IFormFile? ImageFile { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }
    }

}
