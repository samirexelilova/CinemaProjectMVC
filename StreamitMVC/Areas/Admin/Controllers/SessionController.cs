using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.Services;
using StreamitMVC.Services.Interfaces;
using StreamitMVC.ViewModels;
using System;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class SessionController : Controller
    {
        private readonly AppDbContext _context;
        private IPricingService _pricingService;

        public SessionController(AppDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }
        public async Task<IActionResult> Index()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.HallType) 
                .Include(s => s.Cinema)
                .Include(s => s.Language)
                .Include(s => s.HallPrice)
                .Select(s => new GetSessionVM
                {
                    Id = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieName = s.Movie.Name,
                    HallName = s.Hall.Name,
                    CinemaName = s.Cinema.Name,
                    LanguageName = s.Language.Name,
                    Price = s.HallPrice.Price * s.Hall.HallType.ExtraCharge,
                    AvailableSeats = s.AvailableSeats
                })
                .ToListAsync();

            return View(sessions);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new CreateSessionVM
            {
                Movies = await _context.Movies.ToListAsync(),
                Halls = await _context.Halls.Include(h => h.HallType).ToListAsync(), 
                Cinemas = await _context.Cinemas.ToListAsync(),
                HallPrices = await _context.HallPrices.Include(hp => hp.HallType).ToListAsync(),
                Languages = await _context.Languages.ToListAsync(),
                Subtitles = new List<Subtitle>()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSessionVM sessionVM)
        {
            if (!ModelState.IsValid)
            {
                sessionVM.Movies = await _context.Movies.ToListAsync();
                sessionVM.Halls = await _context.Halls.Include(h => h.HallType).ToListAsync();
                sessionVM.Cinemas = await _context.Cinemas.ToListAsync();
                sessionVM.HallPrices = await _context.HallPrices.Include(hp => hp.HallType).ToListAsync();
                sessionVM.Languages = await _context.Languages.ToListAsync();

                if (sessionVM.MovieId != 0 && sessionVM.LanguageId != 0)
                {
                    sessionVM.Subtitles = await _context.Subtitles
                        .Where(s => s.MovieId == sessionVM.MovieId && s.LanguageId == sessionVM.LanguageId)
                        .ToListAsync();
                }
                else
                {
                    sessionVM.Subtitles = new List<Subtitle>();
                }

                return View(sessionVM);
            }

            Hall? hall = await _context.Halls
                .Include(h => h.HallType)
                .FirstOrDefaultAsync(h => h.Id == sessionVM.HallId);

            if (hall == null)
            {
                ModelState.AddModelError(nameof(sessionVM.HallId), "Seçilmiş zal tapılmadı.");

                sessionVM.Movies = await _context.Movies.ToListAsync();
                sessionVM.Halls = await _context.Halls.Include(h => h.HallType).ToListAsync();
                sessionVM.Cinemas = await _context.Cinemas.ToListAsync();
                sessionVM.HallPrices = await _context.HallPrices.Include(hp => hp.HallType).ToListAsync();
                sessionVM.Languages = await _context.Languages.ToListAsync();

                sessionVM.Subtitles = await _context.Subtitles
                    .Where(s => s.MovieId == sessionVM.MovieId && s.LanguageId == sessionVM.LanguageId)
                    .ToListAsync();

                return View(sessionVM);
            }

            Session session = new Session
            {
                StartTime = sessionVM.StartTime,
                EndTime = sessionVM.EndTime,
                MovieId = sessionVM.MovieId,
                HallId = sessionVM.HallId,
                CinemaId = sessionVM.CinemaId,
                HallPriceId = sessionVM.HallPriceId,
                LanguageId = sessionVM.LanguageId,
                AvailableSeats = hall.Capacity,
                SubtitleId = sessionVM.SubtitleId
            };

            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Session? session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            var vm = new UpdateSessionVM
            {
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                MovieId = session.MovieId,
                HallId = session.HallId,
                CinemaId = session.CinemaId,
                HallPriceId = session.HallPriceId,
                LanguageId = session.LanguageId,
                Movies = await _context.Movies.ToListAsync(),
                Halls = await _context.Halls.Include(h => h.HallType).ToListAsync(),
                Cinemas = await _context.Cinemas.ToListAsync(),
                HallPrices = await _context.HallPrices.Include(hp => hp.HallType).ToListAsync(),
                Languages = await _context.Languages.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSessionVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Movies = await _context.Movies.ToListAsync();
                vm.Halls = await _context.Halls.Include(h => h.HallType).ToListAsync();
                vm.Cinemas = await _context.Cinemas.ToListAsync();
                vm.HallPrices = await _context.HallPrices.Include(hp => hp.HallType).ToListAsync();
                vm.Languages = await _context.Languages.ToListAsync();
                return View(vm);
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            session.StartTime = vm.StartTime;
            session.EndTime = vm.EndTime;
            session.MovieId = vm.MovieId;
            session.HallId = vm.HallId;
            session.CinemaId = vm.CinemaId;
            session.HallPriceId = vm.HallPriceId;
            session.LanguageId = vm.LanguageId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Session? session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetSubtitles(int movieId, int languageId)
        {
            if (movieId == 0 || languageId == 0)
            {
                return Json(new List<object>());
            }

            var subtitles = await _context.Subtitles
                .Include(s => s.Language)
                .Where(s => s.MovieId == movieId && s.LanguageId != languageId)
                .Select(s => new 
                {
                    Id = s.Id,
                    Name = s.Language.Name,
                    Code = s.Language.Code
                })
                .ToListAsync();

            return Json(subtitles);
        }
       
    }
}
