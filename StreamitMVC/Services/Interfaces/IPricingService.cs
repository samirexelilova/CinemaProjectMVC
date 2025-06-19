using StreamitMVC.Models;

namespace StreamitMVC.Services.Interfaces
{
    public interface IPricingService
    {
        decimal CalculateSessionPrice(Session session);
    }
}
