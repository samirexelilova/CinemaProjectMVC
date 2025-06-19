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
                    Price = s.HallPrice.Price,
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
                Halls = await _context.Halls.ToListAsync(),
                Cinemas = await _context.Cinemas.ToListAsync(),
                HallPrices = await _context.HallPrices.ToListAsync(),
                Languages = await _context.Languages.ToListAsync()
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSessionVM sessionVM)
        {
            if (!ModelState.IsValid)
            {
                sessionVM.Movies = await _context.Movies.ToListAsync();
                sessionVM.Halls = await _context.Halls.ToListAsync();
                sessionVM.Cinemas = await _context.Cinemas.ToListAsync();
                sessionVM.HallPrices = await _context.HallPrices.ToListAsync();
                sessionVM.Languages = await _context.Languages.ToListAsync();

                return View(sessionVM);
            }

            Hall? hall = await _context.Halls.FirstOrDefaultAsync(h => h.Id == sessionVM.HallId);
            if (hall == null)
            {
                ModelState.AddModelError(nameof(sessionVM.HallId), "Seçilmiş zal tapılmadı.");

                sessionVM.Movies = await _context.Movies.ToListAsync();
                sessionVM.Halls = await _context.Halls.ToListAsync();
                sessionVM.Cinemas = await _context.Cinemas.ToListAsync();
                sessionVM.HallPrices = await _context.HallPrices.ToListAsync();
                sessionVM.Languages = await _context.Languages.ToListAsync();

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
                AvailableSeats = hall.Capacity
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
                Halls = await _context.Halls.ToListAsync(),
                Cinemas = await _context.Cinemas.ToListAsync(),
                HallPrices = await _context.HallPrices.ToListAsync(),
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
                vm.Halls = await _context.Halls.ToListAsync();
                vm.Cinemas = await _context.Cinemas.ToListAsync();
                vm.HallPrices = await _context.HallPrices.ToListAsync();
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
    }
}
