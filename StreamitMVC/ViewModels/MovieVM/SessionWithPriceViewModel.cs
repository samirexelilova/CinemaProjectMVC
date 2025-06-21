using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class SessionWithPriceViewModel
    {
        public int Id { get; set; }
        public Session Session { get; set; }
        public decimal Price { get; set; }
        public List<Session> Sessions { get; set; }
    }
}
