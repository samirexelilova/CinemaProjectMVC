using StreamitMVC.Models;
using StreamitMVC.Services.Interfaces;

namespace StreamitMVC.Services
{
    public class PricingService : IPricingService
    {
        public decimal CalculateSessionPrice(Session session)
        {
            var sessionDay = session.StartTime.DayOfWeek;

            var hallType = session.Hall?.HallType;
            if (hallType == null) return 0;

            var hallPrice = hallType.HallPrices
                .FirstOrDefault(p => p.DayOfWeek == sessionDay || p.DayOfWeek == null);

            if (hallPrice == null) return 0;

            decimal basePrice = hallPrice.Price;
            decimal extra = hallType.ExtraCharge;

            return basePrice + extra;
        }
    }

}
