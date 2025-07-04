using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]

    public class AdminReviewController : Controller
    {
        private readonly AppDbContext _context;

        public AdminReviewController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reviews = _context.Reviews.Include(r => r.Movie).Include(r => r.User).ToList();
            return View(reviews);
        }

        public IActionResult Approve(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();

            review.Status = ReviewStatus.Approved;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reject(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();

            review.Status = ReviewStatus.Rejected;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
