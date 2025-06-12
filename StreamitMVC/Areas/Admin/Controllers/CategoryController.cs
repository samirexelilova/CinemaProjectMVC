using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels.Category;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class CategoryController:Controller
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Categories
             .Include(c => c.MovieCategories)
             .ThenInclude(mc => mc.Movie)
             .ToListAsync();

            List<GetCategoryVM> categoryVMs = categories.Select(c => new GetCategoryVM
            {
                Id = c.Id,
                Name = c.Name,
                Movies = c.MovieCategories.Select(mc => mc.Movie).ToList()
            }).ToList();

            return View(categoryVMs);
        }
        public IActionResult Create()
        {
            CreateCategoryVM model = new CreateCategoryVM
            {
                AllMovies = _context.Movies.ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                model.AllMovies = _context.Movies.ToList(); 
                return View(model);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Update(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Category? category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category is null) return NotFound();

            return View(category);


        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c => c.Name == category.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(Category.Name), $"{category.Name} bu adli category artiq movcuddur");
                return View();
            }
            Category? existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (existed.Name == category.Name) return RedirectToAction(nameof(Index));
            existed.Name = category.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();
            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return NotFound();
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
