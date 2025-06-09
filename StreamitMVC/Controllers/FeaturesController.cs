using Microsoft.AspNetCore.Mvc;

namespace StreamitMVC.Controllers
{
    public class FeaturesController : Controller
    {
        public IActionResult RestrictedContent()
        {
            return View();
        }
        public IActionResult RelatedMerchandise()
        {
            return View();
        }

        public IActionResult Playlist()
        {
            return View();
        }

        public IActionResult Genres()
        {
            return View();
        }

        public IActionResult Cast()
        {
            return View();
        }
    }
}
