using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReviewController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateReviewVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] = "Zəhmət olmasa formu düzgün doldurun.";
                return RedirectToAction("Detail", "MovieIndex", new { id = vm.MovieId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var movie = await _context.Movies.FindAsync(vm.MovieId);
            if (movie == null)
                return NotFound();

            var review = new Review
            {
                MovieId = vm.MovieId,
                UserId = user.Id,
                Comment = vm.Comment,
                Rating = vm.Rating,
                Status = ReviewStatus.Pending,
                LikeCount = 0,
                DislikeCount = 0
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "Reyiniz təsdiqlənmək üçün göndərildi.";
            return RedirectToAction("Detail", "MovieIndex", new { id = vm.MovieId });
        }
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            var existingReaction = await _context.ReviewReactions
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.UserId == user.Id);

            if (existingReaction != null)
                return RedirectToAction("Detail", "MovieIndex", new { id = review.MovieId });

            review.LikeCount++;

            var reaction = new ReviewReaction
            {
                ReviewId = id,
                UserId = user.Id,
                IsLike = true
            };

            _context.ReviewReactions.Add(reaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "MovieIndex", new { id = review.MovieId });
        }

        [HttpPost]
        public async Task<IActionResult> Dislike(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            var existingReaction = await _context.ReviewReactions
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.UserId == user.Id);

            if (existingReaction != null)
                return RedirectToAction("Detail", "MovieIndex", new { id = review.MovieId });

            review.DislikeCount++;

            var reaction = new ReviewReaction
            {
                ReviewId = id,
                UserId = user.Id,
                IsLike = false
            };

            _context.ReviewReactions.Add(reaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "MovieIndex", new { id = review.MovieId });
        }
    }
}
