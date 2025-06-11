using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class MovieController : Controller
    {
        private readonly AppDbContext _context;

        public MovieController(AppDbContext context)
        {
            _context=context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MoviesByDate(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
            {
                // Tarix seçilməyibsə, bəlkə default olaraq bugünkü tarixi göstər
                selectedDate = DateTime.Today;
            }

            // Seçilen tarixə aid seansları olan filmləri çəkmək
            var movies = _context.Movies
             .Include(m => m.Sessions)
                 .ThenInclude(s => s.Hall)
             .Where(m => m.Sessions.Any(s => s.StartTime.Date == selectedDate.Value.Date))
             .ToList();

            // Seçilen tarix ViewModel-də saxlanacaq
            var model = new MoviesByDateViewModel
            {
                SelectedDate = selectedDate.Value,
                Movies = movies
            };

            return View(model);
        }

    }
}
