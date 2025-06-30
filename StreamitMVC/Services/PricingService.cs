using StreamitMVC.Models;
using StreamitMVC.Services.Interfaces;

namespace StreamitMVC.Services
{
    public class PricingService : IPricingService
    {
        public decimal CalculateSessionPrice(Session session)
        {
            if (session.Hall?.HallType == null || session.HallPrice == null)
                return 0;

            return session.HallPrice.Price * session.Hall.HallType.ExtraCharge;
        }

    }

}
