using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SeatController : Controller
    {
        private readonly AppDbContext _context;
        public SeatController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int hallId)
        {
            var hall = await _context.Halls.FindAsync(hallId);
            if (hall == null) return NotFound();

            var seats = await _context.Seats
                .Where(s => s.HallId == hallId)
                .Include(s => s.SeatType)
                .ToListAsync();

            var seatTypes = await _context.SeatTypes.ToListAsync();

            var model = new SeatIndexVM
            {
                Hall = hall,
                Seats = seats,
                SeatTypes = seatTypes
            };

            return View(model);
        }

        public async Task<IActionResult> Create(int hallId, int row, int number)
        {
            var hall = await _context.Halls.FindAsync(hallId);
            if (hall == null) return NotFound();

            bool exists = await _context.Seats.AnyAsync(s => s.HallId == hallId && s.RowNumber == row && s.SeatNumber == number);
            if (exists)
            {
                TempData["Error"] = "Bu yerdə artıq oturacaq mövcuddur!";
                return RedirectToAction("Index", new { hallId });
            }

            var seatTypes = await _context.SeatTypes.ToListAsync();

            var model = new SeatCreateVM
            {
                HallId = hallId,
                HallName = hall.Name,
                RowNumber = row,
                SeatNumber = number,
                SeatTypes = seatTypes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SeatCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                model.SeatTypes = await _context.SeatTypes.ToListAsync();
                return View(model);
            }

            bool exists = await _context.Seats.AnyAsync(s => s.HallId == model.HallId &&
                                                           s.RowNumber == model.RowNumber &&
                                                           s.SeatNumber == model.SeatNumber);
            if (exists)
            {
                TempData["Error"] = "Bu yerdə artıq oturacaq mövcuddur!";
                return RedirectToAction("Index", new { hallId = model.HallId });
            }

            var seat = new Seat
            {
                HallId = model.HallId,
                RowNumber = model.RowNumber,
                SeatNumber = model.SeatNumber,
                SeatTypeId = model.SeatTypeId
            };

            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oturacaq uğurla əlavə edildi!";
            return RedirectToAction("Index", new { hallId = model.HallId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var seat = await _context.Seats
                .Include(s => s.SeatType)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seat == null) return NotFound();

            var seatTypes = await _context.SeatTypes.ToListAsync();

            var model = new SeatEditVM
            {
                HallId = seat.HallId,
                HallName = seat.Hall.Name,
                RowNumber = seat.RowNumber,
                SeatNumber = seat.SeatNumber,
                SeatTypeId = seat.SeatTypeId,
                SeatTypes = seatTypes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SeatEditVM model,int? id )
        {
            if (!ModelState.IsValid)
            {
                model.SeatTypes = await _context.SeatTypes.ToListAsync();
                return View(model);
            }

            var seat = await _context.Seats.FindAsync(id);
            if (seat == null) return NotFound();

            seat.SeatTypeId = model.SeatTypeId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oturacaq uğurla yeniləndi!";
            return RedirectToAction("Index", new { hallId = seat.HallId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var seat = await _context.Seats
                .Include(s => s.SeatType)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seat == null) return NotFound();

            var model = new SeatDeleteVM
            {
                Id = seat.Id,
                HallId = seat.HallId,
                HallName = seat.Hall.Name,
                RowNumber = seat.RowNumber,
                SeatNumber = seat.SeatNumber,
                SeatTypeName = seat.SeatType.Name
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat == null) return NotFound();

            var hallId = seat.HallId;
            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oturacaq uğurla silindi!";
            return RedirectToAction("Index", new { hallId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeType(int seatId, int seatTypeId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null) return NotFound();

            seat.SeatTypeId = seatTypeId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oturacaq tipi dəyişdirildi!";
            return RedirectToAction("Index", new { hallId = seat.HallId });
        }
    }

}
