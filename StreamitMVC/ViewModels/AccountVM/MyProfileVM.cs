using StreamitMVC.ViewModels.AccountVM;

namespace StreamitMVC.ViewModels
{
    public class MyProfileVM
    {
        public UserProfileViewModel User { get; set; }
        public List<BookingViewModel> Bookings { get; set; }
        public List<ReviewViewModel> Reviews { get; set; }
        public AccountDetailsViewModel AccountDetails { get; set; }
    }
}
