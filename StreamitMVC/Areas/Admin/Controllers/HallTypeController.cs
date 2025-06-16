using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class HallTypeController : Controller
    {
        private readonly AppDbContext _context;

        public HallTypeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var hallTypes = await _context.HallTypes
                 .Where(ht => !ht.IsDeleted)
                 .Select(ht => new GetHallTypeVM
                 {
                     Id = ht.Id,
                     Name = ht.Name,
                     ExtraCharge = ht.ExtraCharge
                 })
                 .AsNoTracking()
                 .ToListAsync();

            return View(hallTypes);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateHallTypeVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool exists = await _context.HallTypes.AnyAsync(ht => ht.Name == model.Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), $"{model.Name} artıq mövcuddur");
                return View(model);
            }

            var hallType = new HallType
            {
                Name = model.Name,
                ExtraCharge = model.ExtraCharge,
                CreatedAt = DateTime.Now,
                IsDeleted=false
            };

            _context.HallTypes.Add(hallType);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var hallType = await _context.HallTypes.FirstOrDefaultAsync(ht => ht.Id == id);
            if (hallType == null) return NotFound();

            var model = new UpdateHallTypeVM
            {
                Name = hallType.Name,
                ExtraCharge = hallType.ExtraCharge
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateHallTypeVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool exists = await _context.HallTypes
                .AnyAsync(ht => ht.Name == model.Name && ht.Id != id);

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), $"{model.Name} artıq mövcuddur");
                return View(model);
            }

            var hallType = await _context.HallTypes.FirstOrDefaultAsync(ht => ht.Id ==id);
            if (hallType == null) return NotFound();

            hallType.Name = model.Name;
            hallType.ExtraCharge = model.ExtraCharge;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var hallType = await _context.HallTypes.FirstOrDefaultAsync(ht => ht.Id == id);
            if (hallType == null) return NotFound();

            _context.HallTypes.Remove(hallType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
