using Microsoft.AspNetCore.Mvc;

namespace StreamitMVC.Controllers
{
    public class FeaturesController : Controller
    {
        public IActionResult RestrictedContent()
        {
            return View();
        }
        public IActionResult WatchList()
        {
            return View();
        }

        public IActionResult Genres()
        {
            return View();
        }

        public IActionResult Actors()
        {
            return View();
        }
        public IActionResult Tags()
        {
            return View();
        }
    }
}
