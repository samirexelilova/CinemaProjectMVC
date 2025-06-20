﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

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

            Category category = new Category
            {
                Name = model.Name,
                MovieCategories = model.SelectedMovieIds.Select(id => new MovieCategory
                {
                    MovieId = id
                }).ToList()
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("index");
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
                Name = category.Name,
                SelectedMovieIds = category.MovieCategories.Select(mc => mc.MovieId).ToList(),
                AllMovies = await _context.Movies.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateCategoryVM model)
        {
            if (id is null || id <= 0) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.AllMovies = await _context.Movies.ToListAsync();
                return View(model);
            }

            Category? existed = await _context.Categories
                .Include(c => c.MovieCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            existed.Name = model.Name;

            _context.MovieCategories.RemoveRange(existed.MovieCategories);

            if (model.SelectedMovieIds != null)
            {
                existed.MovieCategories = model.SelectedMovieIds
                    .Select(movieId => new MovieCategory
                    {
                        MovieId = movieId,
                        CategoryId = existed.Id
                    }).ToList();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Category? category = await _context.Categories
                .Include(c => c.MovieCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
