using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null)
            {
                TempData["Error"] = "Səbətiniz boşdur";
                return View(new Basket()); 
            }

            return View(basket);
        }
    }
}
