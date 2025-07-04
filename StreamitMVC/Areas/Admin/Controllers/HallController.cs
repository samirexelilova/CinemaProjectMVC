using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;
using System.Runtime.Intrinsics.Arm;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class HallController : Controller
    {
        private readonly AppDbContext _context;

        public HallController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var halls = await _context.Halls
                .Include(h => h.Cinema)
                .Include(h => h.HallType)
                .Select(h => new GetHallVM
                {
                    Id = h.Id,
                    Name = h.Name,
                    CinemaName = h.Cinema.Name,
                    Capacity = h.Capacity,
                    HallTypeName = h.HallType.Name
                })
                .ToListAsync();

            return View(halls);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CreateHallVM
            {
                Cinemas = await _context.Cinemas.ToListAsync(),
                HallTypes = await _context.HallTypes.ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateHallVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Cinemas = await _context.Cinemas.ToListAsync();
                model.HallTypes = await _context.HallTypes.ToListAsync();
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(CreateHallVM.Name), "Ad boş ola bilməz.");

            if (model.Rows <= 0)
                ModelState.AddModelError(nameof(CreateHallVM.Rows), "Sətir sayı 0‑dan böyük olmalıdır.");

            if (model.SeatsPerRow <= 0)
                ModelState.AddModelError(nameof(CreateHallVM.SeatsPerRow), "Sətirdə oturacaq sayı 0‑dan böyük olmalıdır.");

            bool dup = await _context.Halls.AnyAsync(h =>
                 h.CinemaId == model.CinemaId &&
                 h.Name.ToLower() == model.Name.Trim().ToLower());

            if (dup)
                ModelState.AddModelError(nameof(CreateHallVM.Name), "Bu kinoteatrda həmin adla zal artıq var.");

            var hall = new Hall
            {
                Name = model.Name,
                CinemaId = model.CinemaId,
                Capacity = model.Capacity,
                Rows = model.Rows,
                SeatsPerRow = model.SeatsPerRow,
                HallTypeId = model.HallTypeId
            };

            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            await CreateSeatsForHallAsync(hall);

            return RedirectToAction(nameof(Index));
        }

        private async Task CreateSeatsForHallAsync(Hall hall)
        {
            var emptySeatType = await _context.SeatTypes.FirstOrDefaultAsync(st => st.Name == "Empty");
            if (emptySeatType == null)
            {
                emptySeatType = new SeatType { Name = "Empty", Color = "#FFFFFF" };
                _context.SeatTypes.Add(emptySeatType);
                await _context.SaveChangesAsync();
            }

            for (int row = 1; row <= hall.Rows; row++)
            {
                for (int seatNum = 1; seatNum <= hall.SeatsPerRow; seatNum++)
                {
                    var seat = new Seat
                    {
                        HallId = hall.Id,
                        RowNumber = row,
                        SeatNumber = seatNum,
                        SeatTypeId = emptySeatType.Id 
                    };
                    _context.Seats.Add(seat);
                }
            }
            await _context.SaveChangesAsync();
        }


        public async Task<IActionResult> Update(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            var model = new UpdateHallVM
            {
                Name = hall.Name,
                CinemaId = hall.CinemaId,
                Capacity = hall.Capacity,
                Rows = hall.Rows,
                SeatsPerRow = hall.SeatsPerRow,
                HallTypeId = hall.HallTypeId,
                Cinemas = await _context.Cinemas.ToListAsync(),
                HallTypes = await _context.HallTypes.ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateHallVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Cinemas = await _context.Cinemas.ToListAsync();
                model.HallTypes = await _context.HallTypes.ToListAsync();
                return View(model);
            }

            bool exists = await _context.Halls
                .AnyAsync(h => h.Id != id
                           && h.CinemaId == model.CinemaId
                           && h.Name.ToLower() == model.Name.Trim().ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(UpdateHallVM.Name), "Bu adla zal artıq mövcuddur.");
                model.Cinemas = await _context.Cinemas.ToListAsync();
                model.HallTypes = await _context.HallTypes.ToListAsync();
                return View(model);
            }

            if (model.Rows <= 0)
                ModelState.AddModelError(nameof(UpdateHallVM.Rows), "Sətir sayı 0-dan böyük olmalıdır.");

            if (model.SeatsPerRow <= 0)
                ModelState.AddModelError(nameof(UpdateHallVM.SeatsPerRow), "Sətirdə oturacaq sayı 0-dan böyük olmalıdır.");

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            bool cinemaExists = await _context.Cinemas.AnyAsync(c => c.Id == model.CinemaId);
            if (!cinemaExists)
                ModelState.AddModelError(nameof(UpdateHallVM.CinemaId), "Seçilmiş kinoteatr mövcud deyil.");

            bool hallTypeExists = await _context.HallTypes.AnyAsync(ht => ht.Id == model.HallTypeId);
            if (!hallTypeExists)
                ModelState.AddModelError(nameof(UpdateHallVM.HallTypeId), "Seçilmiş zal tipi mövcud deyil.");

            if (!ModelState.IsValid)
            {
                model.Cinemas = await _context.Cinemas.ToListAsync();
                model.HallTypes = await _context.HallTypes.ToListAsync();
                return View(model);
            }

            hall.Name = model.Name;
            hall.CinemaId = model.CinemaId;
            hall.Capacity = model.Capacity;
            hall.Rows = model.Rows;
            hall.SeatsPerRow = model.SeatsPerRow;
            hall.HallTypeId = model.HallTypeId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var hall = await _context.Halls
                .Include(h => h.Cinema)
                .Include(h => h.HallType)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hall == null)
                return NotFound();

            var model = new GetHallVM
            {
                Id = hall.Id,
                Name = hall.Name,
                CinemaName = hall.Cinema.Name,
                Capacity = hall.Capacity,
                HallTypeName = hall.HallType.Name
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            var seats = _context.Seats.Where(s => s.HallId == id);
            _context.Seats.RemoveRange(seats);

            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}
