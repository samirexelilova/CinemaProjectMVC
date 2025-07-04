using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Movie> movies = _context.Movies
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .Include(m => m.MovieLanguages).ThenInclude(ml => ml.Language)
                .Include(m => m.Subtitles).ThenInclude(su => su.Language)
                .Include(m => m.MovieTags).ThenInclude(mt => mt.Tag)
                .OrderByDescending(m => m.ImdbRating).Take(3)
                .ToList();

            List<Movie> upcomingMovies = _context.Movies
            .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
            .Include(m => m.MovieLanguages).ThenInclude(ml => ml.Language)
            .Include(m => m.Subtitles).ThenInclude(su => su.Language)
            .Include(m => m.MovieTags).ThenInclude(mt => mt.Tag)
            .Where(m => m.ReleaseDate > DateTime.Now) 
            .OrderBy(m => m.ReleaseDate) 
            .Take(10) 
            .ToList();

            var latestMovies =  _context.Movies
              .Where(m => !m.IsDeleted)
             .OrderByDescending(m => m.ReleaseDate)
             .Take(10)
              .ToList();

            var suggestedMovies =  _context.Movies
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt) 
            .Take(6)
            .Include(m => m.MovieTags).ThenInclude(mt => mt.Tag)
            .ToList();
            var currentYear = DateTime.Now.Year;

            var topMovieOfYear =  _context.Movies
                .Where(m => m.ReleaseDate.Year == currentYear && !m.IsDeleted)
                .OrderByDescending(m => m.ImdbRating)
                .FirstOrDefault();
            HomeVM homeVM = new HomeVM
            {
                Movies = movies,
                Movie = movies.FirstOrDefault() ,
                UpcomingMovies = upcomingMovies,
                LatestMovies = latestMovies,
                SuggestedMovies = suggestedMovies,
                TopMovieOfYear = topMovieOfYear,
            };

            return View(homeVM);
        }


    }
}
