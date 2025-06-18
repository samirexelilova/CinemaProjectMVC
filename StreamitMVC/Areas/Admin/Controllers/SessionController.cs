using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]
    public class SessionController : Controller
    {
        private readonly AppDbContext _context;
        public SessionController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var sessions = _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .Include(s => s.HallPrice)
                .Include(s => s.Language)
                .ToList();

            return View(sessions);
        }
        public IActionResult Create()
        {
            var viewModel = new CreateSessionVM
            {
                Movies = _context.Movies.ToList(),
                Halls = _context.Halls.ToList(),
                Cinemas = _context.Cinemas.ToList(),
                HallPrices = _context.HallPrices.ToList(),
                Languages = _context.Languages.ToList(),
                Session = new Session()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(CreateSessionVM viewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Sessions.Add(viewModel.Session);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            viewModel.Movies = _context.Movies.ToList();
            viewModel.Halls = _context.Halls.ToList();
            viewModel.Cinemas = _context.Cinemas.ToList();
            viewModel.HallPrices = _context.HallPrices.ToList();
            viewModel.Languages = _context.Languages.ToList();

            return View(viewModel);
        }
    }
}
