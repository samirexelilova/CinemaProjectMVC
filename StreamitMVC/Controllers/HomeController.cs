using Microsoft.AspNetCore.Mvc;

namespace StreamitMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MovieIndex()
        {
            return View();
        }
    }
}
