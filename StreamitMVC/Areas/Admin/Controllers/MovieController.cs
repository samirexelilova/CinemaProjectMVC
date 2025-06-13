using Microsoft.AspNetCore.Mvc;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels.CreateMovieVM;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class MovieController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MovieController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            var vm = new CreateMovieVM
            {
                AllCategories = _context.Categories.ToList(),
                AllTags = _context.Tags.ToList(),
                AllActors = _context.Actors.ToList(),
                AllLanguages = _context.Languages.ToList()
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateMovieVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var movie = new Movie
            {
                Name = vm.Name,
                Photo = await vm.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies","popular"),
                TrailerVideo =await vm.TrailerVideoFile.CreateFileAsync(_env.WebRootPath, "assets", "images", "video"),
                VideoUrl =await vm.VideoUrlFile.CreateFileAsync(_env.WebRootPath, "assets", "images", "video"),
                Duration = vm.Duration,
                Director = vm.Director,
                Country = vm.Country,
                ImdbRating = vm.ImdbRating,
                Description = vm.Description,
                AgeRestriction = vm.AgeRestriction,
                ReleaseDate = vm.ReleaseDate,
                LanguageId = vm.LanguageId,
                MovieCategories = vm.SelectedCategoryIds.Select(id => new MovieCategory { CategoryId = id }).ToList(),
                MovieTags = vm.SelectedTagIds.Select(id => new MovieTag { TagId = id }).ToList(),
                MovieActors = vm.SelectedActorIds.Select(id => new MovieActor { ActorId = id }).ToList()
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
