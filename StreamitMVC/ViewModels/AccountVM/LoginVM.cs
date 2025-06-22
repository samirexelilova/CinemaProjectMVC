using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "İstifadəçi adı və ya email boş ola bilməz")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Şifrə boş ola bilməz")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsPersistent { get; set; }  
    }
}
