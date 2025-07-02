using StreamitMVC.Extensions.Enums;

namespace StreamitMVC.ViewModels;

public class UserProfileViewModel
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public Gender? Gender { get; set; }
    public string? Image { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public decimal? WalletBalance { get; set; }


}
