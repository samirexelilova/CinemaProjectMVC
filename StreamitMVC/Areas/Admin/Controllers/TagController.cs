using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;
        public TagController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetTagVM> tagVMs = await _context.Tags
                .Where(c => !c.IsDeleted)
                .Include(c => c.MovieTags)
                .AsNoTracking()
                .Select(p => new GetTagVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    MovieCount = p.MovieTags.Count 
                })
                .ToListAsync();

            return View(tagVMs);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Tag tag = new Tag
            {
                Name = model.Name
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction("index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Tag? tag = await _context.Tags
                .Include(c => c.MovieTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (tag is null) return NotFound();

            UpdateTagVM vm = new UpdateTagVM
            {
                Name = tag.Name
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateTagVM model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Tags.AnyAsync(p => p.Name == model.Name && p.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateTagVM.Name), $"{model.Name} bu adli tag artiq movcuddur");
                return View();
            }
            Tag? existed = await _context.Tags.FirstOrDefaultAsync(p => p.Id == id);

            if (existed.Name == model.Name) return RedirectToAction(nameof(Index));
            existed.Name = model.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Tag? existed = await _context.Tags.FirstOrDefaultAsync(p => p.Id == id);

            if (existed is null) return NotFound();

            _context.Tags.Remove(existed);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
