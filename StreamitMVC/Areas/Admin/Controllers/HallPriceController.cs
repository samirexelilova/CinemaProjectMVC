using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HallPriceController : Controller
    {
        private readonly AppDbContext _context;

        public HallPriceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var prices = await _context.HallPrices
                .Include(hp => hp.HallType)
                .Where(hp => !hp.IsDeleted)
                .Select(hp => new GetHallPrice
                {
                    Id = hp.Id,
                    HallTypeName = hp.HallType.Name,
                    Price = hp.Price,
                    DayOfWeek = hp.DayOfWeek
                })
                .ToListAsync();

            return View(prices);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CreateHallPrice
            {
                HallTypes = await _context.HallTypes.Where(ht => !ht.IsDeleted).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateHallPrice model)
        {
            if (!ModelState.IsValid)
            {
                model.HallTypes = await _context.HallTypes.Where(ht => !ht.IsDeleted).ToListAsync();
                return View(model);
            }

            var hallPrice = new HallPrice
            {
                HallTypeId = model.HallTypeId,
                Price = model.Price,
                DayOfWeek = model.DayOfWeek,
                CreatedAt = DateTime.Now
            };

            _context.HallPrices.Add(hallPrice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var hallPrice = await _context.HallPrices.FirstOrDefaultAsync(x => x.Id == id);
            if (hallPrice is null) return NotFound();

            var model = new UpdateHallPrice
            {
                HallTypeId = hallPrice.HallTypeId,
                Price = hallPrice.Price,
                DayOfWeek = hallPrice.DayOfWeek,
                HallTypes = await _context.HallTypes.Where(ht => !ht.IsDeleted).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateHallPrice model)
        {
            if (!ModelState.IsValid)
            {
                model.HallTypes = await _context.HallTypes.Where(ht => !ht.IsDeleted).ToListAsync();
                return View(model);
            }

            var hallPrice = await _context.HallPrices.FirstOrDefaultAsync(x => x.Id == id);
            if (hallPrice is null) return NotFound();

            hallPrice.HallTypeId = model.HallTypeId;
            hallPrice.Price = model.Price;
            hallPrice.DayOfWeek = model.DayOfWeek;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            var hallPrice = await _context.HallPrices.FirstOrDefaultAsync(x => x.Id == id);
            if (hallPrice is null) return NotFound();

            _context.HallPrices.Remove(hallPrice);  
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
