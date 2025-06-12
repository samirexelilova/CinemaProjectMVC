using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class MovieController : Controller
    {
        private readonly AppDbContext _context;

        public MovieController(AppDbContext context)
        {
            _context = context;
        }
      


          public IActionResult Index()
           {
            var movies = _context.Movies
                .Include(m => m.Language)
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
                .Include(m=>m.Language)
                .Include(m=>m.Subtitles)
                .Include(m=>m.Sessions)
                .Include(m=>m.MovieActors)
                .ThenInclude(m=>m.Actor)
                .Include(m=>m.Reviews)
                .Include(m => m.MovieTags)
                 .ThenInclude(m => m.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (movie is null) return NotFound();

            var categoryIds = movie.MovieCategories.Select(mc => mc.CategoryId).ToList();


            DetailVM detailVm = new DetailVM()
            {
                Movie = movie,
                RelatedMovies = await _context.Movies
                       .Where(m => m.Id != movie.Id &&
                  m.MovieCategories.Any(mc => categoryIds.Contains(mc.CategoryId)))
                 .ToListAsync()
            };
            return View(detailVm);
        }

    }

}
