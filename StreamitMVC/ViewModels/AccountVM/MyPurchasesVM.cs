using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class MyPurchasesVM
    {
        public List<MoviePurchase> ActivePurchases { get; set; }
        public List<MoviePurchase> ExpiredPurchases { get; set; }
    }
}
