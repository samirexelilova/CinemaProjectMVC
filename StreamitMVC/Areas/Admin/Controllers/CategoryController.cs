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
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index()
        {
            List<GetCategoryVM> categoryVMs = await _context.Categories
                 .Where(c => !c.IsDeleted)
                 .Include(c => c.MovieCategories)
                 .AsNoTracking()
                 .Select(p => new GetCategoryVM
                 {
                     Id = p.Id,
                     Name = p.Name,
                     MovieCount = p.MovieCategories.Count
                 })
                .ToListAsync();
            return View(categoryVMs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool nameExists = await _context.Categories.AnyAsync(c => c.Name == model.Name);
            if (nameExists)
            {
                ModelState.AddModelError(nameof(CreateCategoryVM.Name), $"{model.Name} bu adlı category artıq mövcuddur");
                return View(model); 
            }

            Category category = new Category
            {
                Name = model.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Category? category = await _context.Categories
                .Include(c => c.MovieCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null) return NotFound();

            UpdateCategoryVM vm = new UpdateCategoryVM
            {
                Name = category.Name
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateCategoryVM model)
        {
            if (id is null || id <= 0) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool result = await _context.Categories.AnyAsync(p => p.Name == model.Name && p.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateCategoryVM.Name), $"{model.Name} bu adlı category artıq mövcuddur");
                return View(model); 
            }

            Category? existed = await _context.Categories.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            if (existed.Name == model.Name) return RedirectToAction(nameof(Index));

            existed.Name = model.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Category? existed = await _context.Categories.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            _context.Categories.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}