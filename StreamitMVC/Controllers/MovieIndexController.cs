using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
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
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieTags)
                    .ThenInclude(mt => mt.Tag)
                .Include(m => m.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie is null) return NotFound();

            var sessions = await _context.Sessions
                 .Where(s => s.MovieId == movie.Id)
                 .Include(s => s.Hall)
                    .ThenInclude(h => h.HallType)
                        .ThenInclude(ht => ht.HallPrices)
                 .Include(s => s.HallPrice)
                  .Include(s => s.Cinema)
                .Include(s => s.Language)
               .Include(s => s.Subtitle)
                .ThenInclude(st => st.Language)
                 .ToListAsync();

            var sessionPrices = sessions
                .Select(s => new SessionWithPriceViewModel
                {
                    Session = s,
                    Price = _pricingService.CalculateSessionPrice(s)
                }).ToList();

            var categoryIds = movie.MovieCategories.Select(mc => mc.CategoryId).ToList();
            var relatedMovies = await _context.Movies
                .Where(m => m.Id != movie.Id &&
                    m.MovieCategories.Any(mc => categoryIds.Contains(mc.CategoryId)))
                .ToListAsync();

            DetailVM detailVm = new DetailVM
            {
                Movie = movie,
                RelatedMovies = relatedMovies,
                SessionPrices = sessionPrices
            };

            return View(detailVm);
        }



    }

}
