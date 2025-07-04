using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StreamitMVC.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }

    }
}
