using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;

        public MovieIndexController(AppDbContext context, IPricingService pricingService, UserManager<AppUser> userManager)
        {
            _context = context;
            _pricingService = pricingService;
            _userManager = userManager;

        }
        public async Task<IActionResult> Index(int? SelectedLanguageId, int? SelectedCinemaId, DateTime? SelectedDate, bool isAjax = false)
        {
            var moviesQuery = _context.Movies
                .Include(m => m.MovieLanguages).ThenInclude(ml => ml.Language)
                .Include(m => m.Sessions)
                .AsQueryable();

            if (SelectedLanguageId != null)
                moviesQuery = moviesQuery.Where(m => m.MovieLanguages.Any(ml => ml.LanguageId == SelectedLanguageId));

            if (SelectedCinemaId != null)
                moviesQuery = moviesQuery.Where(m => m.Sessions.Any(s => s.CinemaId == SelectedCinemaId));

            if (SelectedDate != null)
                moviesQuery = moviesQuery.Where(m => m.Sessions.Any(s => s.StartTime.Date == SelectedDate.Value.Date));

            var latestMovies = _context.Movies
          .Where(m => !m.IsDeleted)
         .OrderByDescending(m => m.ReleaseDate)
         .Take(10)
          .ToList();

            var suggestedMovies = _context.Movies
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Take(6)
            .Include(m => m.MovieTags).ThenInclude(mt => mt.Tag)
            .ToList();
            var vm = new MovieVM
            {
                Movies = await moviesQuery.ToListAsync(),
                MovieSwipper = await _context.Movies
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .Include(m => m.MovieLanguages).ThenInclude(ml => ml.Language)
                .Include(m => m.Subtitles).ThenInclude(su => su.Language)
                .Include(m => m.MovieTags).ThenInclude(mt => mt.Tag)
                .OrderByDescending(m => m.ImdbRating).Take(3)
                .ToListAsync(),
                Languages = await _context.Languages.ToListAsync(),
                Cinemas = await _context.Cinemas.ToListAsync(),
                Slides = await _context.Slides.ToListAsync(),
                SelectedCinemaId = SelectedCinemaId,
                SelectedLanguageId = SelectedLanguageId,
                SelectedDate = SelectedDate,
                LatestMovies =latestMovies,
                SuggestedMovies = suggestedMovies
            };

            if (isAjax)
            {
                return PartialView("MovieListHtml", vm); 
            }

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
    .Include(s => s.Movie)
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

            bool isMovieFavorited = false;
            List<Favorite> userFavorites = new List<Favorite>();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                userFavorites = await _context.Favorites
                    .Include(f => f.Movie)
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                isMovieFavorited = userFavorites.Any(f => f.MovieId == movie.Id);
            }
            var languages = await _context.Languages.ToListAsync();
            var cinemas = await _context.Cinemas.ToListAsync();
            DetailVM detailVm = new DetailVM
            {
                Movie = movie,
                RelatedMovies = relatedMovies,
                SessionPrices = sessionPrices,
                Favorites = userFavorites,
                IsMovieFavorited = isMovieFavorited,
                Languages = languages,
                Cinemas = cinemas
            };

            return View(detailVm);
        }
        public async Task<IActionResult> FilterSessions(int id, int? SelectedLanguageId = null, int? SelectedCinemaId = null, DateTime? SelectedDate = null)
        {
            var sessionsQuery = _context.Sessions
      .Where(s => s.MovieId == id)
      .Include(s => s.Movie)
      .Include(s => s.Hall)
          .ThenInclude(h => h.HallType)
              .ThenInclude(ht => ht.HallPrices)
      .Include(s => s.HallPrice)
      .Include(s => s.Cinema)
      .Include(s => s.Language)
      .Include(s => s.Subtitle)
          .ThenInclude(st => st.Language)
      .AsQueryable();
            if (SelectedLanguageId != null)
                sessionsQuery = sessionsQuery.Where(s => s.LanguageId == SelectedLanguageId);

            if (SelectedCinemaId != null)
                sessionsQuery = sessionsQuery.Where(s => s.CinemaId == SelectedCinemaId);

            if (SelectedDate != null)
                sessionsQuery = sessionsQuery.Where(s => s.StartTime.Date == SelectedDate.Value.Date);

            var sessions = await sessionsQuery.ToListAsync();

            var sessionPrices = sessions
                .Select(s => new SessionWithPriceViewModel
                {
                    Session = s,
                    Price = _pricingService.CalculateSessionPrice(s)
                }).ToList();

            return PartialView("_SessionListPartial", sessionPrices);
        }



    }
}
