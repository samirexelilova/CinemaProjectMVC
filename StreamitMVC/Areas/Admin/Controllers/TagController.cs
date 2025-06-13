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
            List<Tag> tags = await _context.Tags
             .Include(c => c.MovieTags)
             .ThenInclude(mc => mc.Movie)
             .ToListAsync();

            List<GetTagVM> tagVm = tags.Select(c => new GetTagVM
            {
                Id = c.Id,
                Name = c.Name,
                Movies = c.MovieTags.Select(mc => mc.Movie).ToList()
            }).ToList();

            return View(tagVm);
        }
        public IActionResult Create()
        {
            CreateTagVM model = new CreateTagVM
            {
                AllMovies = _context.Movies.ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM model)
        {
            if (!ModelState.IsValid)
            {
                model.AllMovies = _context.Movies.ToList();
                return View(model);
            }

            Tag tag = new Tag
            {
                Name = model.Name,
                MovieTags = model.SelectedMovieIds.Select(id => new MovieTag
                {
                    MovieId = id
                }).ToList()
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
                Name = tag.Name,
                SelectedMovieIds = tag.MovieTags.Select(mc => mc.MovieId).ToList(),
                AllMovies = await _context.Movies.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateTagVM model)
        {
            if (id is null || id <= 0) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.AllMovies = await _context.Movies.ToListAsync();
                return View(model);
            }

            Tag? existed = await _context.Tags
                .Include(c => c.MovieTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            existed.Name = model.Name;

            _context.MovieTags.RemoveRange(existed.MovieTags);

            if (model.SelectedMovieIds != null)
            {
                existed.MovieTags = model.SelectedMovieIds
                    .Select(movieId => new MovieTag
                    {
                        MovieId = movieId,
                        TagId = existed.Id
                    }).ToList();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Tag? tag = await _context.Tags
                .Include(c => c.MovieTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (tag is null) return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
