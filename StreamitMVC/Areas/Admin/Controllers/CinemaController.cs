using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Migrations;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CinemaController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<GetCinemaVM> cinemaVMs = await _context.Cinemas.Select(s =>
            new GetCinemaVM
            {
                Id = s.Id,
                Name = s.Name,
                Photo = s.Photo,
                Address=s.Address,
                ContactNumber = s.ContactNumber,
                CloseTime = s.CloseTime,
                OpenTime = s.OpenTime
            }

            ).ToListAsync();
            return View(cinemaVMs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCinemaVM cinemaVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool title = await _context.Cinemas.AnyAsync(s => s.Name == cinemaVM.Name);
            if (title)
            {
                ModelState.AddModelError(nameof(CreateCinemaVM.Name), $"{cinemaVM.Name} Bu cinema artiq movcuddur");
                return View();
            }
            if (cinemaVM.Photo == null)
            {
                ModelState.AddModelError(nameof(CreateCinemaVM.Photo), "Zəhmət olmasa şəkil seçin.");
                return View();
            }

            if (!cinemaVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateCinemaVM.Photo), " Bu File Type duzgun deyil");
                return View();
            }
            if (!cinemaVM.Photo.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateCinemaVM.Photo), " File size 2 mb dan boyuk ola bilmez ");
                return View();
            }


            string fileName = await cinemaVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "cinema");

            Cinema cinema = new Cinema()
            {
                Name = cinemaVM.Name,
                Photo = fileName,
                Address = cinemaVM.Address,
                ContactNumber = cinemaVM.ContactNumber,
                OpenTime = cinemaVM.OpenTime,
                CloseTime = cinemaVM.CloseTime,
            };

            await _context.Cinemas.AddAsync(cinema);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Cinema? cinema = await _context.Cinemas.FirstOrDefaultAsync(s => s.Id == id);
            if (cinema is null) return NotFound();
            UpdateCinemaVM updateCinemaVM = new UpdateCinemaVM
            {
                Name = cinema.Name,
                Image = cinema.Photo,
                Address = cinema.Address,
                ContactNumber = cinema.ContactNumber,
                OpenTime = cinema.OpenTime,
                CloseTime = cinema.CloseTime
            };
            return View(updateCinemaVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateCinemaVM updateCinemaVM)
        {
            if (!ModelState.IsValid)
            {
                return View(updateCinemaVM);
            }
            Cinema? existed = await _context.Cinemas.FirstOrDefaultAsync(s => s.Id == id);

            if (updateCinemaVM.Photo is not null)
            {
                if (!updateCinemaVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateCinemaVM.Photo), "Sekil type duzgun deyil");
                    return View(updateCinemaVM);
                }
                if (!updateCinemaVM.Photo.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateCinemaVM.Photo), "Sekil 1 mb dan boyuk ola bilmez");
                    return View(updateCinemaVM);
                }
                string fileName = await updateCinemaVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "cinema");
                existed.Photo.DeleteFile(_env.WebRootPath, "assets", "images", "cinema");
                existed.Photo = fileName;
            }
            existed.Name=updateCinemaVM.Name;
            existed.Address=updateCinemaVM.Address;
            existed.ContactNumber=updateCinemaVM.ContactNumber;
            existed.OpenTime=updateCinemaVM.OpenTime;
            existed.CloseTime=updateCinemaVM.CloseTime;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int ?id)
        {
            if (id is null || id <= 0) return BadRequest();
            Cinema? cinema = await _context.Cinemas.FirstOrDefaultAsync(s => s.Id == id);
            if (cinema is null) return NotFound();

            cinema.Photo.DeleteFile(_env.WebRootPath, "assets", "images", "cinema");
            _context.Cinemas.Remove(cinema);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}
