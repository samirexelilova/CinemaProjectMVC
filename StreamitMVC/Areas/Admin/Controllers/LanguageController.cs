using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;
using System.Drawing;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class LanguageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LanguageController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
           List<GetLanguageVM> languages = await _context.Languages
         .Include(l => l.MovieLanguages)
         .ThenInclude(ml => ml.Movie)
         .Select(l => new GetLanguageVM
         {
             Id = l.Id,
             Name = l.Name,
             Code = l.Code,
             FlagImage = l.FlagIcon,
             Movies = l.MovieLanguages.Select(ml => ml.Movie).ToList()
         })
         .ToListAsync();

            return View(languages);
        }
        public IActionResult Create()
        {
            CreateLanguageVM model = new CreateLanguageVM
            {
                AllMovies = _context.Movies.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLanguageVM model)
        {
            if (!ModelState.IsValid) 
            {
                model.AllMovies = _context.Movies.ToList();
                return View(model);
            }

            if (model.FlagImage == null)
            {
                ModelState.AddModelError(nameof(CreateLanguageVM.FlagImage), "Zəhmət olmasa şəkil seçin.");
                return View();
            }

            if (!model.FlagImage.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateLanguageVM.FlagImage), " Bu File Type duzgun deyil");
                return View();
            }
            if (!model.FlagImage.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateLanguageVM.FlagImage), " File size 2 mb dan boyuk ola bilmez ");
                return View();
            }
            string fileName = await model.FlagImage.CreateFileAsync(_env.WebRootPath, "assets", "images");
            Language language = new Language
            {
                Name = model.Name,
                Code = model.Code,
                FlagIcon = fileName,
                MovieLanguages = model.SelectedMovieIds.Select(id => new MovieLanguage
                {
                    MovieId = id
                }).ToList()
            };

            await _context.Languages.AddAsync(language);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Language? language = await _context.Languages
                .Include(c => c.MovieLanguages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (language is null) return NotFound();

            var vm = new UpdateLanguageVM
            {
                Name = language.Name,
                Code = language.Code,
                ExistedFlagImage = language.FlagIcon,
                SelectedMovieIds = language.MovieLanguages.Select(mc => mc.MovieId).ToList(),
                AllMovies = await _context.Movies.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateLanguageVM model)
        {
            if (id is null || id <= 0) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.AllMovies = await _context.Movies.ToListAsync();
                return View(model);
            }

            Language? existed = await _context.Languages
                .Include(c => c.MovieLanguages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            if (model.FlagImage is not null)
            {
                if (!model.FlagImage.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateLanguageVM.FlagImage), "Sekil type duzgun deyil");
                    return View(model);
                }
                if (!model.FlagImage.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateLanguageVM.FlagImage), "Sekil 1 mb dan boyuk ola bilmez");
                    return View(model);
                }
                string fileName = await model.FlagImage.CreateFileAsync(_env.WebRootPath, "assets", "images");
                existed.FlagIcon.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.FlagIcon = fileName;
            }

            existed.Name = model.Name;
            existed.Code = model.Code;

            _context.MovieLanguages.RemoveRange(existed.MovieLanguages);

            if (model.SelectedMovieIds != null)
            {
                existed.MovieLanguages = model.SelectedMovieIds
                    .Select(movieId => new MovieLanguage
                    {
                        MovieId = movieId,
                        LanguageId = existed.Id
                    }).ToList();
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Language? language = await _context.Languages
                .Include(c => c.MovieLanguages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (language == null) return NotFound();

             language.FlagIcon.DeleteFile(_env.WebRootPath, "assets", "images");
            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
