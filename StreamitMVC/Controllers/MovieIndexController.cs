using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.Services;
using StreamitMVC.Services.Interfaces;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class MovieIndexController : Controller
    {
        private readonly AppDbContext _context;
        private IPricingService _pricingService;

        public MovieIndexController(AppDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }



        public IActionResult Index()
        {
            var movies = _context.Movies
            .Include(m => m.MovieLanguages)
             .ThenInclude(ml => ml.Language)
           .ToList();

            MovieVM vm = new MovieVM
            {
                Movies = movies,
                Languages = _context.Languages.ToList(),
                Cinemas = _context.Cinemas.ToList(),
                Slides = _context.Slides.OrderBy(s => s.Order).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Movie? movie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Include(m => m.MovieLanguages)
                    .ThenInclude(ml => ml.Language)
                .Include(m => m.Subtitles)
                    .ThenInclude(su => su.Language)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.HallType)
                            .ThenInclude(ht => ht.HallPrices)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.HallPrice)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Cinema)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Language)
                .Include(m => m.MovieActors)
                    .ThenInclude(m => m.Actor)
                .Include(m => m.MovieTags)
                    .ThenInclude(m => m.Tag)
               .Include(m => m.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (movie is null) return NotFound();

            var categoryIds = movie.MovieCategories.Select(mc => mc.CategoryId).ToList();

            var sessionPrices = movie.Sessions
                .Select(s => new SessionWithPriceViewModel
                {
                    Session = s,
                    Price = _pricingService.CalculateSessionPrice(s)
                }).ToList();

            DetailVM detailVm = new DetailVM()
            {
                Movie = movie,
                RelatedMovies = await _context.Movies
                    .Where(m => m.Id != movie.Id &&
                        m.MovieCategories.Any(mc => categoryIds.Contains(mc.CategoryId)))
                    .ToListAsync(),
                SessionPrices = sessionPrices
            };

            return View(detailVm);
        }


    }

}
