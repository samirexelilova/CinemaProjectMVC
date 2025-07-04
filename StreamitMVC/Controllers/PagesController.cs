using Microsoft.AspNetCore.Mvc;

namespace StreamitMVC.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult FAQ()
        {
            return View();
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
       
    }
}
