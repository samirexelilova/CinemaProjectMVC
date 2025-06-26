namespace StreamitMVC.ViewModels.AccountVM
{
    public class AccountDetailsViewModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
