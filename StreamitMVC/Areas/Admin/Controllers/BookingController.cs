using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]

    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;

        public BookingController(AppDbContext context, IWebHostEnvironment env,UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }
        public async Task<IActionResult> SelectSeats(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Seats)
                        .ThenInclude(seat => seat.SeatType)
                .Include(s => s.Cinema)
                .Include(s => s.HallPrice)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            var takenSeatIds = session.Tickets
                .Where(t => t.Status == TicketStatus.Reserved || t.Status == TicketStatus.Sold)
                .Select(t => t.SeatId)
                .ToList();

            var model = new SelectSeatsVM
            {
                SessionId = session.Id,
                HallName = session.Hall.Name,
                Rows = session.Hall.Rows,
                SeatsPerRow = session.Hall.SeatsPerRow,
                Movie = session.Movie,
                Session = session,
                Seats = session.Hall.Seats
                    .Select(seat => new SeatViewModel
                    {
                        Id = seat.Id,
                        Row = seat.RowNumber,
                        Number = seat.SeatNumber,
                        Type = seat.SeatType.Name,
                        Color = seat.SeatType.Color,
                        IsTaken = takenSeatIds.Contains(seat.Id)
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSeats(int sessionId, List<int> SelectedSeatIds)
        {
            if (SelectedSeatIds == null || !SelectedSeatIds.Any())
            {
                TempData["Error"] = "Zəhmət olmasa en azı bir oturacaq seçin";
                return RedirectToAction("SelectSeats", new { sessionId });
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.HallPrice)
                .Include(s => s.Movie)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                TempData["Error"] = "Seans tapılmadı";
                return RedirectToAction("Index", "Home");
            }

            var selectedSeats = await _context.Seats
                .Where(s => SelectedSeatIds.Contains(s.Id))
                .ToListAsync();

            var alreadyTakenSeats = session.Tickets
                .Where(t => SelectedSeatIds.Contains(t.SeatId) &&
                           (t.Status == TicketStatus.Reserved || t.Status == TicketStatus.Sold))
                .Select(t => t.SeatId)
                .ToList();

            if (alreadyTakenSeats.Any())
            {
                TempData["Error"] = "Seçdiyiniz oturacaqlardan bəziləri artıq rezerv edilib";
                return RedirectToAction("SelectSeats", new { sessionId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Bilet rezerv etmək üçün login olmalısınız";
                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = user.Id,
                    Items = new List<BasketItem>()
                };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            decimal seatPrice = session.HallPrice.Price; 

            foreach (var seatId in SelectedSeatIds)
            {
                var existingItem = basket.Items
                    .FirstOrDefault(i => i.SessionId == sessionId && i.SeatId == seatId);

                if (existingItem == null)
                {
                    var basketItem = new BasketItem
                    {
                        BasketId = basket.Id,
                        SessionId = sessionId,
                        SeatId = seatId,
                        Price = seatPrice
                    };

                    _context.BasketItems.Add(basketItem);
                }
            }

            await _context.SaveChangesAsync();

            basket.TotalPrice = await _context.BasketItems
                .Where(bi => bi.BasketId == basket.Id)
                .SumAsync(bi => bi.Price);

            _context.Baskets.Update(basket);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{SelectedSeatIds.Count} oturacaq səbətə əlavə edildi";
            return RedirectToAction("Index", "Basket", new { area = "" });
        }

        public async Task<IActionResult> ViewBasket()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromBasket(int basketItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Istifadeci tapılmadı" });
            }

            var basketItem = await _context.BasketItems
                .Include(bi => bi.Basket)
                .FirstOrDefaultAsync(bi => bi.Id == basketItemId && bi.Basket.UserId == user.Id);

            if (basketItem == null)
            {
                return Json(new { success = false, message = "Element tapılmadı" });
            }

            _context.BasketItems.Remove(basketItem);

            var basket = basketItem.Basket;
            basket.TotalPrice -= basketItem.Price;

            if (basket.TotalPrice < 0)
                basket.TotalPrice = 0;

            _context.Baskets.Update(basket);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Element səbətdən silindi" });
        }

        [HttpPost]
        public async Task<IActionResult> ClearBasket()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false });
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket != null)
            {
                _context.BasketItems.RemoveRange(basket.Items);
                basket.TotalPrice = 0;
                _context.Baskets.Update(basket);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbətiniz boşdu";
                return RedirectToAction("Index", "Home");
            }

            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Istifadeci tapılmadı" });
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                return Json(new { success = false, message = "Səbət bosdu" });
            }

            var bookings = new List<Booking>();

            foreach (var item in basket.Items)
            {
                var existingBooking = bookings.FirstOrDefault(b => b.SessionId == item.SessionId);
                if (existingBooking == null)
                {
                    var newBooking = new Booking
                    {
                        UserId = user.Id,
                        SessionId = item.SessionId,
                        BookingDate = DateTime.Now,
                        Status = BookingStatus.Reserved,
                        TotalAmount = item.Price,
                        Tickets = new List<Ticket>()
                    };

                    var ticket = new Ticket
                    {
                        SeatId = item.SeatId,
                        SessionId = item.SessionId,
                        Price = item.Price,
                        Status = TicketStatus.Reserved
                    };

                    newBooking.Tickets.Add(ticket);
                    bookings.Add(newBooking);
                }
                else
                {
                    var ticket = new Ticket
                    {
                        SeatId = item.SeatId,
                        SessionId = item.SessionId,
                        Price = item.Price,
                        Status = TicketStatus.Reserved
                    };

                    existingBooking.Tickets.Add(ticket);
                    existingBooking.TotalAmount += item.Price;
                }
            }

            await _context.Bookings.AddRangeAsync(bookings);

            _context.BasketItems.RemoveRange(basket.Items);
            basket.TotalPrice = 0;
            _context.Baskets.Update(basket);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Ödəniş tamamlandı və biletler rezerv olundu" });
        }

    }
}
