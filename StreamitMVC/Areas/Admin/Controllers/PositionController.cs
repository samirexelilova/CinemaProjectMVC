using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]

    public class PositionController : Controller
    {
        private readonly AppDbContext _context;
        public PositionController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetPositionVM> positionVMs = await _context.Positions
         .Where(c => c.IsDeleted == false)
         .Include(c => c.Actors)
         .AsNoTracking()
         .Select(p => new GetPositionVM
         {
             Id = p.Id,
             Name = p.Name,
             Actors=p.Actors.ToList()
         })
         .ToListAsync();

            return View(positionVMs);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreatePositionVM positionVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Positions.AnyAsync(p => p.Name == positionVM.Name);
            if (result)
            {
                ModelState.AddModelError(nameof(CreatePositionVM.Name), $"{positionVM.Name} bu ad artiq movcuddur");
                return View();
            }
            Position position = new Position
            {
                Name = positionVM.Name,
                CreatedAt = DateTime.Now
            };

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Position? position = _context.Positions.FirstOrDefault(p => p.Id == id);
            if (position is null) return NotFound();
            UpdatePositionVM vm = new UpdatePositionVM
            {
                Name = position.Name
            };
            return View(vm);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdatePositionVM positionVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Positions.AnyAsync(p => p.Name == positionVM.Name && p.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdatePositionVM.Name), $"{positionVM.Name} bu adli position artiq movcuddur");
                return View();
            }
            Position? existed = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id);

            if (existed.Name == positionVM.Name) return RedirectToAction(nameof(Index));
            existed.Name = positionVM.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Position? existed = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            _context.Positions.Remove(existed);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
