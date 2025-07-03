using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.ViewModels;
using StreamitMVC.Models;
using StreamitMVC.DAL;
using Microsoft.AspNetCore.Hosting;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.Utilities.Enums;

namespace StreamitMVC.Controllers
{
    [Area("admin")]
    public class MovieController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MovieController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var movies = _context.Movies
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .Include(m => m.MovieTags).ThenInclude(mt => mt.Tag)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieLanguages).ThenInclude(ml => ml.Language)
                .Include(m => m.Subtitles).ThenInclude(s => s.Language)
                .Include(m => m.Sessions)
                .Include(m => m.Reviews)
                .Select(m => new GetMovieVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    Photo = m.Photo,
                    TrailerVideo = m.TrailerVideo,
                    VideoUrl = m.VideoUrl,
                    Duration = m.Duration,
                    Director = m.Director,
                    Country = m.Country,
                    ImdbRating = m.ImdbRating,
                    AgeRestriction = m.AgeRestriction,
                    ReleaseDate = m.ReleaseDate,

                    Categories = m.MovieCategories.Select(mc => mc.Category).ToList(),
                    Tags = m.MovieTags.Select(mt => mt.Tag).ToList(),
                    Actors = m.MovieActors.Select(ma => ma.Actor).ToList(),
                    Languages = m.MovieLanguages.Select(ml => ml.Language).ToList(),
                    Subtitles = m.Subtitles.Select(s => s.Language).ToList(),

                    SessionCount = m.Sessions.Count,
                    ReviewCount = m.Reviews.Count
                })
                .ToList();

            return View(movies);
        }
        public async Task<IActionResult> Create()
        {
            var vm = new CreateMovieVM
            {
                Categories = await _context.Categories.ToListAsync(),
                Actors = await _context.Actors.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Languages = await _context.Languages.ToListAsync(),
                Subtitles = await _context.Subtitles.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMovieVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _context.Categories.ToListAsync();
                vm.Actors = await _context.Actors.ToListAsync();
                vm.Tags = await _context.Tags.ToListAsync();
                vm.Languages = await _context.Languages.ToListAsync();
                vm.Subtitles = await _context.Subtitles.ToListAsync();

                return View(vm);
            }


            string photoPath = null;
            if (vm.PhotoFile != null)
            {
                if (!vm.PhotoFile.ValidateType("image/"))
                {
                    ModelState.AddModelError("PhotoFile", "Yalnız şəkil faylları yükləyə bilərsiniz.");
                    return View(vm);
                }

                if (!vm.PhotoFile.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError("PhotoFile", "Faylın ölçüsü maksimum 2MB ola bilər.");
                    return View(vm);

                }

                photoPath = await vm.PhotoFile.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies", "popular");
            }

            string videoPath = null;
            if (vm.VideoFile != null)
            {
                if (!vm.VideoFile.ValidateType("video/"))
                {
                    ModelState.AddModelError("VideoFile", "Yalnız video faylları yükləyə bilərsiniz.");
                    return View(vm);

                }

                if (!vm.VideoFile.ValidateSize(FileSize.MB, 50))
                {
                    ModelState.AddModelError("VideoFile", "Video maksimum 50MB ola bilər.");
                    return View(vm);

                }

                videoPath = await vm.VideoFile.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies");
            }

            string trailerPath = null;
            if (vm.TrailerVideo != null)
            {
                if (!vm.TrailerVideo.ValidateType("video/"))
                {
                    ModelState.AddModelError("TrailerVideoFile", "Yalnız video faylları yükləyə bilərsiniz.");
                    return View(vm);

                }

                if (!vm.TrailerVideo.ValidateSize(FileSize.MB, 50))
                {
                    ModelState.AddModelError("TrailerVideoFile", "Video maksimum 50MB ola bilər.");
                }

                trailerPath = await vm.TrailerVideo.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies");

            }

            var movie = new Movie
            {
                Name = vm.Name,
                Photo = photoPath,
                VideoUrl = videoPath,
                TrailerVideo = trailerPath,
                Duration = vm.Duration,
                Director = vm.Director,
                Country = vm.Country,
                ImdbRating = vm.ImdbRating,
                Description = vm.Description,
                AgeRestriction = vm.AgeRestriction,
                ReleaseDate = vm.ReleaseDate,
                MovieCategories = vm.SelectedCategoryIds.Select(id => new MovieCategory { CategoryId = id }).ToList(),
                MovieTags = vm.SelectedTagIds.Select(id => new MovieTag { TagId = id }).ToList(),
                MovieActors = vm.ActorRoles
              ?.Where(x => x.ActorId > 0 && !string.IsNullOrWhiteSpace(x.Role))
             .Select(x => new MovieActor
             {
                    ActorId = x.ActorId,
                Role = x.Role
              }).ToList(),
                MovieLanguages = vm.SelectedLanguageIds.Select(id => new MovieLanguage { LanguageId = id }).ToList(),
                Subtitles = _context.Subtitles.Where(s => vm.SelectedSubtitleIds.Contains(s.Id)).ToList()
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

      

        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .Include(m => m.MovieTags)
                .Include(m => m.MovieActors)
                .Include(m => m.MovieLanguages)
                .Include(m => m.Sessions)
                .Include(m => m.Reviews)
                .Include(m => m.Subtitles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            movie.Photo?.DeleteFile(_env.WebRootPath, "assets", "images", "movies", "popular");

            movie.VideoUrl?.DeleteFile(_env.WebRootPath, "assets", "images", "movies");
            movie.TrailerVideo?.DeleteFile(_env.WebRootPath, "assets", "images", "movies");

            _context.Sessions.RemoveRange(movie.Sessions);

            _context.Reviews.RemoveRange(movie.Reviews);

            _context.MovieCategories.RemoveRange(movie.MovieCategories);
            _context.MovieTags.RemoveRange(movie.MovieTags);
            _context.MovieActors.RemoveRange(movie.MovieActors);
            _context.MovieLanguages.RemoveRange(movie.MovieLanguages);


            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id <= 0)
                return BadRequest();

            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .Include(m => m.MovieTags)
                .Include(m => m.MovieActors)
                .Include(m => m.MovieLanguages)
                .Include(m => m.Subtitles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            var vm = new UpdateMovieVM
            {
                Name = movie.Name,
                PhotoPath = movie.Photo,
                TrailerVideoPath = movie.TrailerVideo,
                VideoPath = movie.VideoUrl,
                Duration = movie.Duration,
                Director = movie.Director,
                Country = movie.Country,
                ImdbRating = movie.ImdbRating,
                Description = movie.Description,
                AgeRestriction = movie.AgeRestriction,
                ReleaseDate = movie.ReleaseDate,

                SelectedCategoryIds = movie.MovieCategories.Select(mc => mc.CategoryId).ToList(),
                SelectedTagIds = movie.MovieTags.Select(mt => mt.TagId).ToList(),
                SelectedLanguageIds = movie.MovieLanguages.Select(ml => ml.LanguageId).ToList(),
                SelectedSubtitleIds = movie.Subtitles.Select(s => s.Id).ToList(),

                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Actors = await _context.Actors.ToListAsync(),
                Languages = await _context.Languages.ToListAsync(),
                Subtitles = await _context.Subtitles.ToListAsync(),

                ActorRoles = movie.MovieActors.Select(ma => new UpdateMovieVM.ActorWithRole
                {
                    ActorId = ma.ActorId,
                    Role = ma.Role
                }).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateMovieVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _context.Categories.ToListAsync();
                vm.Tags = await _context.Tags.ToListAsync();
                vm.Actors = await _context.Actors.ToListAsync();
                vm.Languages = await _context.Languages.ToListAsync();
                vm.Subtitles = await _context.Subtitles.ToListAsync();

                return View(vm);
            }

            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .Include(m => m.MovieTags)
                .Include(m => m.MovieActors)
                .Include(m => m.MovieLanguages)
                .Include(m => m.Subtitles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            // Yalnız doldurulmuş şəkili yenilə
            if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
            {
                if (!vm.PhotoFile.ValidateType("image/"))
                {
                    ModelState.AddModelError("PhotoFile", "Yalnız şəkil faylları yükləyə bilərsiniz.");
                    return View(vm);
                }
                if (!vm.PhotoFile.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError("PhotoFile", "Faylın ölçüsü maksimum 2MB ola bilər.");
                    return View(vm);
                }

                movie.Photo?.DeleteFile(_env.WebRootPath, "assets", "images", "movies", "popular");
                movie.Photo = await vm.PhotoFile.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies", "popular");
            }

            // Yalnız doldurulmuş traileri yenilə
            if (vm.TrailerVideo != null && vm.TrailerVideo.Length > 0)
            {
                if (!vm.TrailerVideo.ValidateType("video/"))
                {
                    ModelState.AddModelError("TrailerVideo", "Yalnız video faylları yükləyə bilərsiniz.");
                    return View(vm);
                }
                if (!vm.TrailerVideo.ValidateSize(FileSize.MB, 50))
                {
                    ModelState.AddModelError("TrailerVideo", "Video maksimum 50MB ola bilər.");
                    return View(vm);
                }

                movie.TrailerVideo?.DeleteFile(_env.WebRootPath, "assets", "images", "movies");
                movie.TrailerVideo = await vm.TrailerVideo.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies");
            }

            // Yalnız doldurulmuş videonu yenilə
            if (vm.VideoFile != null && vm.VideoFile.Length > 0)
            {
                if (!vm.VideoFile.ValidateType("video/"))
                {
                    ModelState.AddModelError("VideoFile", "Yalnız video faylları yükləyə bilərsiniz.");
                    return View(vm);
                }
                if (!vm.VideoFile.ValidateSize(FileSize.MB, 50))
                {
                    ModelState.AddModelError("VideoFile", "Video maksimum 50MB ola bilər.");
                    return View(vm);
                }

                movie.VideoUrl?.DeleteFile(_env.WebRootPath, "assets", "images", "movies");
                movie.VideoUrl = await vm.VideoFile.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies");
            }

            // Digər məlumatları yenilə
            movie.Name = vm.Name;
            movie.Duration = vm.Duration;
            movie.Director = vm.Director;
            movie.Country = vm.Country;
            movie.ImdbRating = vm.ImdbRating;
            movie.Description = vm.Description;
            movie.AgeRestriction = vm.AgeRestriction;
            movie.ReleaseDate = vm.ReleaseDate;

            // Əlaqəli məlumatları yenilə
            _context.MovieCategories.RemoveRange(movie.MovieCategories);
            _context.MovieTags.RemoveRange(movie.MovieTags);
            _context.MovieActors.RemoveRange(movie.MovieActors);
            _context.MovieLanguages.RemoveRange(movie.MovieLanguages);
            movie.Subtitles.Clear();

            movie.MovieCategories = vm.SelectedCategoryIds?.Select(id => new MovieCategory { CategoryId = id, MovieId = movie.Id }).ToList() ?? new List<MovieCategory>();
            movie.MovieTags = vm.SelectedTagIds?.Select(id => new MovieTag { TagId = id, MovieId = movie.Id }).ToList() ?? new List<MovieTag>();
            movie.MovieActors = vm.ActorRoles
                ?.Where(x => x.ActorId > 0 && !string.IsNullOrWhiteSpace(x.Role))
                .Select(x => new MovieActor
                {
                    ActorId = x.ActorId,
                    Role = x.Role,
                    MovieId = movie.Id
                }).ToList() ?? new List<MovieActor>();
            movie.MovieLanguages = vm.SelectedLanguageIds?.Select(id => new MovieLanguage { LanguageId = id, MovieId = movie.Id }).ToList() ?? new List<MovieLanguage>();

            var selectedSubtitles = await _context.Subtitles.Where(s => vm.SelectedSubtitleIds.Contains(s.Id)).ToListAsync();
            movie.Subtitles = selectedSubtitles;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}