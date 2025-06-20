using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class SubtitleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SubtitleController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var subtitles = await _context.Subtitles
                .Include(s => s.Movie)
                .Include(s => s.Language)
                .Select(s => new GetSubtitleVM
                {
                    Id = s.Id,
                    MovieName = s.Movie.Name,
                    LanguageName = s.Language.Name,
                    FilePath = s.FilePath
                })
                .ToListAsync();

            return View(subtitles);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CreateSubtitleVM
            {
                AllMovies = await _context.Movies.ToListAsync(),
                AllLanguages = await _context.Languages.ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubtitleVM model)
        {
            if (!ModelState.IsValid)
            {
                model.AllMovies = await _context.Movies.ToListAsync();
                model.AllLanguages = await _context.Languages.ToListAsync();
                return View(model);
            }

            if (!model.SubtitleFile.ValidateType("text/") && !model.SubtitleFile.ValidateType("application/octet-stream"))
            {
                ModelState.AddModelError(nameof(model.SubtitleFile), "Subtitrlər üçün fayl tipi düzgün deyil.");
                model.AllMovies = await _context.Movies.ToListAsync();
                model.AllLanguages = await _context.Languages.ToListAsync();
                return View(model);
            }

            if (!model.SubtitleFile.ValidateSize(FileSize.MB, 5))  
            {
                ModelState.AddModelError(nameof(model.SubtitleFile), "Fayl ölçüsü 5MB-dan böyük ola bilməz.");
                model.AllMovies = await _context.Movies.ToListAsync();
                model.AllLanguages = await _context.Languages.ToListAsync();
                return View(model);
            }

            string fileName = await model.SubtitleFile.CreateFileAsync(_env.WebRootPath, "assets", "subtitles");

            Subtitle subtitle = new Subtitle
            {
                MovieId = model.MovieId,
                LanguageId = model.LanguageId,
                FilePath = fileName
            };

            await _context.Subtitles.AddAsync(subtitle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var subtitle = await _context.Subtitles
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subtitle == null) return NotFound();

            var model = new UpdateSubtitleVM
            {
                MovieId = subtitle.MovieId,
                LanguageId = subtitle.LanguageId,
                ExistedFilePath = subtitle.FilePath,
                AllMovies = await _context.Movies.ToListAsync(),
                AllLanguages = await _context.Languages.ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSubtitleVM model)
        {
            if (id == null || id <= 0) return BadRequest();

            // Əvvəlcə drop-down-lar üçün lazımi məlumatları yüklə (view üçün)
            model.AllMovies = await _context.Movies.ToListAsync();
            model.AllLanguages = await _context.Languages.ToListAsync();

            if (!ModelState.IsValid)
            {
                // Əgər model valid deyil, view-ə qaytar
                return View(model);
            }

            var subtitle = await _context.Subtitles.FirstOrDefaultAsync(s => s.Id == id);
            if (subtitle == null) return NotFound();

            // Fayl seçilibsə yoxlamaları et
            if (model.SubtitleFile != null)
            {
                // Fayl tipi yoxla
                if (!model.SubtitleFile.ValidateType("text/") && !model.SubtitleFile.ValidateType("application/octet-stream"))
                {
                    ModelState.AddModelError(nameof(model.SubtitleFile), "Subtitrlər üçün fayl tipi düzgün deyil.");
                    return View(model);
                }

                // Fayl ölçüsü yoxla
                if (!model.SubtitleFile.ValidateSize(FileSize.MB, 5))
                {
                    ModelState.AddModelError(nameof(model.SubtitleFile), "Fayl ölçüsü 5MB-dan böyük ola bilməz.");
                    return View(model);
                }

                // Faylı yüklə və köhnəsini sil
                string fileName = await model.SubtitleFile.CreateFileAsync(_env.WebRootPath, "assets", "subtitles");
                subtitle.FilePath.DeleteFile(_env.WebRootPath, "assets", "subtitles");
                subtitle.FilePath = fileName;
            }

            // Digər sahələri yenilə
            subtitle.MovieId = model.MovieId;
            subtitle.LanguageId = model.LanguageId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var subtitle = await _context.Subtitles.FirstOrDefaultAsync(s => s.Id == id);
            if (subtitle == null) return NotFound();

            subtitle.FilePath.DeleteFile(_env.WebRootPath, "assets", "subtitles");
            _context.Subtitles.Remove(subtitle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
