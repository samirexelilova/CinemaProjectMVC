using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class FeaturesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public FeaturesController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context=context;
            _userManager = userManager;
        }
        public IActionResult RestrictedContent()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorited = false, message = "Movie removed from favorites" });
            }
            else
            {
                var fav = new Favorite
                {
                    MovieId = movieId,
                    UserId = userId
                };
                _context.Favorites.Add(fav);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorited = true, message = "Movie added to favorites" });
            }
        }

        public async Task<IActionResult> Favorites()
        {
            var userId = _userManager.GetUserId(User);
            var favorites = await _context.Favorites
                .Include(f => f.Movie)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return View(favorites);
        }
        public IActionResult Genres()
        {
            return View();
        }
        public IActionResult Actors(int page = 1)
        {
            int pageSize = 12; 

            var actors = _context.Actors
                                 .Include(x => x.Position)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            int totalActors = _context.Actors.Count();
            int totalPages = (int)Math.Ceiling((double)totalActors / pageSize);

            ActorVm vm = new ActorVm
            {
                Actors = actors,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }
        public async Task<IActionResult> ActorDetail(int? id)
        {
            if (id == null) return BadRequest();

            var actor = await _context.Actors
                .Include(a => a.Position)
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                        .ThenInclude(m => m.MovieStats)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null) return NotFound();

            var allMovies = new List<Movie>();
            var mostViewedMovies = new List<Movie>();

            if (actor.MovieActors != null && actor.MovieActors.Any())
            {
                allMovies = actor.MovieActors
                    .Where(ma => ma?.Movie != null)
                    .Select(ma => ma.Movie)
                    .ToList();

                mostViewedMovies = allMovies
                    .OrderByDescending(m => m.MovieStats?.ViewCount ?? 0)
                    .Take(5)
                    .ToList();
            }

            var vm = new ActorVm
            {
                Actor = actor,
                Movies = allMovies,
                MostViewedMovies = mostViewedMovies
            };

            return View(vm);
        }
        public IActionResult Tags()
        {
            return View();
        }
    }
}
