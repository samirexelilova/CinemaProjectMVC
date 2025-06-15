using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Migrations;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;
using StreamitMVC.ViewModels.ActorVM;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class ActorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ActorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            var actors = await _context.Actors
                .Include(a => a.Position)
                .Include(a => a.MovieActors)
                .Include(a => a.SocialMedias)
                .Select(a => new GetActorVM
                {
                    Id = a.Id,
                    Name = a.Name,
                    Surname = a.Surname,
                    Photo = a.Photo,
                    PositionName = a.Position.Name,
                    Award = a.Award,
                    Description = a.Description,
                    Born = a.Born,
                    FilmCount = a.MovieActors.Count,
                    SocialMedias = a.SocialMedias.Select(sm => new SocialMediaVM
                    {
                        Platform = sm.Platform,
                        Url = sm.Url
                    }).ToList()
                })
                .ToListAsync();

            return View(actors);
        }

        public IActionResult Create()
        {
            CreateActorVM model = new CreateActorVM
            {
                Positions = _context.Positions.ToList(),
                SocialMedias = new List<SocialMediaVM>() 
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateActorVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Positions = _context.Positions.ToList();
                return View(model);
            }
            if (model.Photo == null)
            {
                ModelState.AddModelError(nameof(CreateActorVM.Photo), "Zəhmət olmasa şəkil seçin.");
                return View();
            }

            if (!model.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateActorVM.Photo), " Bu File Type duzgun deyil");
                return View();
            }
            if (!model.Photo.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateActorVM.Photo), " File size 2 mb dan boyuk ola bilmez ");
                return View();
            }
            string photoFileName = await model.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "cast");
            Actor actor = new Actor
            {
                Name = model.Name,
                Surname = model.Surname,
                PositionId = model.PositionId.Value,
                Photo = photoFileName,
                Description = model.Description,
                Award = model.Award,
                Born = model.Born,
                Parents = model.Parents,
                SocialMedias = model.SocialMedias
                    .Where(sm => !string.IsNullOrWhiteSpace(sm.Url))
                    .Select(sm => new ActorSocialMedia
                    {
                        Platform = Enum.Parse<SocialMediaPlatform>(sm.Platform.ToString()),
                        Url = sm.Url
                    })
                    .ToList()
            };

            _context.Actors.Add(actor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Update(int id)
        {
            var actor = await _context.Actors
                .Include(a => a.SocialMedias)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null) return NotFound();

            var model = new UpdateActorVM
            {
                Name = actor.Name,
                Surname = actor.Surname,
                Image = actor.Photo,  
                PositionId = actor.PositionId,
                Positions = _context.Positions.ToList(),
                Description = actor.Description,
                Award = actor.Award,
                Born = actor.Born,
                Parents = actor.Parents,
                SocialMedias = actor.SocialMedias.Select(sm => new SocialMediaVM
                {
                    Platform = sm.Platform,
                    Url = sm.Url
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateActorVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Positions = _context.Positions.ToList();
                return View(model);
            }

            var actor = await _context.Actors
                .Include(a => a.SocialMedias)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null) return NotFound();

            actor.Name = model.Name;
            actor.Surname = model.Surname;
            actor.PositionId = model.PositionId.Value;
            actor.Description = model.Description;
            actor.Award = model.Award;
            actor.Born = model.Born;
            actor.Parents = model.Parents;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                string fileName = await model.Photo.CreateFileAsync(_env.WebRootPath,"assets", "images", "cast");
                actor.Photo.DeleteFile(_env.WebRootPath, "assets", "images", "cast");
                actor.Photo = fileName;
            }

            actor.SocialMedias.Clear();
            if (model.SocialMedias != null)
            {
                actor.SocialMedias = model.SocialMedias
                    .Where(sm => !string.IsNullOrWhiteSpace(sm.Url))
                    .Select(sm => new ActorSocialMedia
                    {
                        Platform = sm.Platform,
                        Url = sm.Url
                    }).ToList();
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var actor = await _context.Actors
                .Include(a => a.SocialMedias)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null) return NotFound();
            actor.Photo.DeleteFile(_env.WebRootPath, "assets", "images", "cast");
            _context.ActorSocialMedias.RemoveRange(actor.SocialMedias);
            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
