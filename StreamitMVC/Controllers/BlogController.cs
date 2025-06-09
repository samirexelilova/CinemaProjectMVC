using Microsoft.AspNetCore.Mvc;

namespace StreamitMVC.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
