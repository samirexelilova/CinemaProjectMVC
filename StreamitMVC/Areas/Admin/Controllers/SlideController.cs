using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var slides = await _context.Slides.OrderBy(s => s.Order).ToListAsync();

            var slideVMs = slides.Select(s => new GetSlideVM
            {
                Id = s.Id,
                Name = s.Name,
                Order = s.Order,
                Image = s.Image,
                CreatedAt = s.CreatedAt,
                TrailerVideo = s.TrailerVideo,
                Duration = s.Duration,
                ImdbRating = s.ImdbRating,
                ReleaseDate = s.ReleaseDate,
                Description = s.Description
            }).ToList();

            return View(slideVMs);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _context.Slides.AnyAsync(s => s.Name == vm.Name))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Name), "Bu ad artiq movcuddur");
                return View(vm);
            }

            if (await _context.Slides.AnyAsync(s => s.Order == vm.Order))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Order), "Bu order artiq movcuddur");
                return View(vm);
            }

            if (!vm.Photo.ValidateType("image/") || !vm.Photo.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Photo), "Sekil uygun deyil");
                return View(vm);
            }

            if (!vm.TrailerVideoURL.ValidateType("video/") || !vm.TrailerVideoURL.ValidateSize(FileSize.MB, 50))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.TrailerVideoURL), "Video uygun deyil");
                return View(vm);
            }

            string imageFile = await vm.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies","popular");
            string trailerFile = await vm.TrailerVideoURL.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies");

            var slide = new Slide
            {
                Name = vm.Name,
                Order = vm.Order,
                Image = imageFile,
                TrailerVideo = trailerFile,
                Duration = vm.Duration,
                ImdbRating = vm.ImdbRating,
                ReleaseDate = vm.ReleaseDate,
                Description = vm.Description,
                CreatedAt = DateTime.Now
            };

            _context.Slides.Add(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return BadRequest();

            var slide = await _context.Slides.FindAsync(id);
            if (slide is null) return NotFound();

            var vm = new UpdateSlideVM
            {
                Name = slide.Name,
                Order = slide.Order,
                Image = slide.Image,
                TrailerVideo = slide.TrailerVideo,
                Duration = slide.Duration,
                ImdbRating = slide.ImdbRating,
                ReleaseDate = slide.ReleaseDate,
                Description = slide.Description
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSlideVM vm)
        {
            if (id is null) return BadRequest();

            var slide = await _context.Slides.FindAsync(id);
            if (slide is null) return NotFound();

            if (!ModelState.IsValid) return View(vm);

            if (vm.Photo != null)
            {
                if (!vm.Photo.ValidateType("image/") || !vm.Photo.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "Sekil uygun deyil");
                    return View(vm);
                }

                string newImage = await vm.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies", "popular");
                slide.Image.DeleteFile(_env.WebRootPath, "assets", "images", "movies", "popular");
                slide.Image = newImage;
            }

            if (vm.TrailerVideoURL != null)
            {
                if (!vm.TrailerVideoURL.ValidateType("video/") || !vm.TrailerVideoURL.ValidateSize(FileSize.MB, 10))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.TrailerVideoURL), "Video uygun deyil");
                    return View(vm);
                }

                string newVideo = await vm.TrailerVideoURL.CreateFileAsync(_env.WebRootPath, "assets", "images", "movies");
                slide.TrailerVideo.DeleteFile(_env.WebRootPath, "assets", "images", "movies");
                slide.TrailerVideo = newVideo;
            }

            slide.Name = vm.Name;
            slide.Order = vm.Order;
            slide.Duration = vm.Duration;
            slide.ImdbRating = vm.ImdbRating;
            slide.ReleaseDate = vm.ReleaseDate;
            slide.Description = vm.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            var slide = await _context.Slides.FindAsync(id);
            if (slide is null) return NotFound();

            slide.Image.DeleteFile(_env.WebRootPath, "assets", "images", "movies", "popular");
            slide.TrailerVideo.DeleteFile(_env.WebRootPath, "assets", "images", "movies");

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
