using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace StreamitMVC.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public BookingController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _configuration = configuration;

        }

        public async Task CleanOldSessions()
        {
            var currentDate = DateTime.Now.Date;
            var oneWeekAgo = currentDate.AddDays(-1);

            var oldSessions = await _context.Sessions
                .Where(s => s.StartTime.Date < currentDate)
                .ToListAsync();

            if (oldSessions.Any())
            {
                var oldSessionIds = oldSessions.Select(s => s.Id).ToList();
                var oldTickets = await _context.Tickets
                    .Where(t => oldSessionIds.Contains(t.SessionId))
                    .ToListAsync();

                _context.Tickets.RemoveRange(oldTickets);

                _context.Sessions.RemoveRange(oldSessions);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> SelectSeats(int sessionId, string error = null)
        {
            await CleanOldSessions();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Seats)
                        .ThenInclude(seat => seat.SeatType)
                .Include(s => s.Cinema)
                .Include(s => s.HallPrice)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound("Seans tapılmadı");

            var currentTime = DateTime.Now;
            var oneWeekLater = currentTime.AddDays(7);

            if (session.StartTime < currentTime)
            {
                TempData["Error"] = "Bu seans artıq başlayıb və ya bitib";
                return RedirectToAction("Index", "Home");
            }

            if (session.StartTime > oneWeekLater)
            {
                TempData["Error"] = "Bu seans çox uzaq gələcəkdədir";
                return RedirectToAction("Index", "Home");
            }

            if (session.StartTime <= currentTime.AddMinutes(30))
            {
                TempData["Error"] = "Bu seansa bilet satışı başa çatıb";
                return RedirectToAction("Index", "Home");
            }

            var takenSeatIds = await GetTakenSeatsForSession(sessionId);

            var model = new SelectSeatsVM
            {
                SessionId = session.Id,
                HallName = session.Hall.Name,
                Rows = session.Hall.Rows,
                SeatsPerRow = session.Hall.SeatsPerRow,
                Movie = session.Movie,
                Session = session,
                Seats = session.Hall.Seats.Select(seat => new SeatViewModel
                {
                    Id = seat.Id,
                    Row = seat.RowNumber,
                    Number = seat.SeatNumber,
                    Type = seat.SeatType.Name,
                    Color = seat.SeatType.Color,
                    IsTaken = takenSeatIds.Contains(seat.Id),
                    IsSold = false
                }).OrderBy(s => s.Row).ThenBy(s => s.Number).ToList(),
                ErrorMessage = error
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSeats(int sessionId, List<int> selectedSeatIds)
        {
            if (selectedSeatIds == null || !selectedSeatIds.Any())
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Zəhmət olmasa ən azı bir oturacaq seçin" });
            }

            if (selectedSeatIds.Count > 6)
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Maksimum 6 oturacaq seçə bilərsiniz" });
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.HallPrice)
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                return RedirectToAction("Index", "Home", new { error = "Seans tapılmadı" });
            }

            if (session.StartTime <= DateTime.Now.AddMinutes(30))
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Bu seansa bilet satışı başa çatıb" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("SelectSeats", new { sessionId }) });
            }

            var takenSeatIds = await GetTakenSeatsForSession(sessionId);
            var conflictingSeats = selectedSeatIds.Intersect(takenSeatIds).ToList();

            if (conflictingSeats.Any())
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Seçdiyiniz oturacaqlardan bəziləri artıq rezerv edilib" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var basket = await GetOrCreateUserBasket(user.Id);

                var booking = new Booking
                {
                    UserId = user.Id,
                    SessionId = sessionId,
                    Status = BookingStatus.Reserved,
                    BookingDate = DateTime.Now,
                    TotalAmount = selectedSeatIds.Count * session.HallPrice.Price
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                decimal seatPrice = session.HallPrice.Price;
                var ticketsToAdd = new List<Ticket>();
                var basketItemsToAdd = new List<BasketItem>();

                foreach (var seatId in selectedSeatIds)
                {
                    var seatTaken = await _context.Tickets.AnyAsync(t =>
                        t.SeatId == seatId &&
                        t.SessionId == sessionId &&
                        (t.Status == TicketStatus.Sold ||
                         (t.Status == TicketStatus.Reserved &&
                          t.PurchasedAt.AddMinutes(15) > DateTime.Now)));

                    if (seatTaken)
                    {
                        await transaction.RollbackAsync();
                        return RedirectToAction("SelectSeats",
                            new { sessionId, error = "Seçdiyiniz oturacaqlardan bəziləri artıq rezerv edilib" });
                    }

                    var ticket = new Ticket
                    {
                        BookingId = booking.Id,
                        SeatId = seatId,
                        SessionId = sessionId,
                        Price = seatPrice,
                        Status = TicketStatus.Reserved,
                        PurchasedAt = DateTime.Now
                    };
                    ticketsToAdd.Add(ticket);

                    var basketItem = new BasketItem
                    {
                        BasketId = basket.Id,
                        SessionId = sessionId,
                        SeatId = seatId,
                        Price = seatPrice
                    };
                    basketItemsToAdd.Add(basketItem);
                }

                _context.Tickets.AddRange(ticketsToAdd);
                _context.BasketItems.AddRange(basketItemsToAdd);

                basket.TotalPrice = await _context.BasketItems
                    .Where(bi => bi.BasketId == basket.Id)
                    .SumAsync(bi => bi.Price) + (basketItemsToAdd.Count * seatPrice);

                _context.Baskets.Update(basket);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"{selectedSeatIds.Count} oturacaq səbətə əlavə edildi. " +
                                    "15 dəqiqə ərzində ödəniş etməzsəniz rezerv ləğv olunacaq.";

                return RedirectToAction("Checkout");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Xəta baş verdi. Yenidən cəhd edin." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await CleanExpiredReservations(user.Id);

            var basket = await _context.Baskets
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Cinema)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                        .ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbətiniz boşdur.";
                return RedirectToAction("Index", "Home");
            }

            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Cinema)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
                return BadRequest("Səbət boşdur");

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = basket.Items.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "azn",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Session.Movie.Name,
                            Description = $"Kinoteatr: {item.Session.Cinema.Name}, Zal: {item.Session.Hall.Name}, Yer: {item.Seat.RowNumber}-{item.Seat.SeatNumber}, Vaxt: {item.Session.StartTime:dd.MM.yyyy HH:mm}"
                        }
                    },
                    Quantity = 1,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + Url.Action("PaymentSuccess", "Booking"),
                CancelUrl = domain + Url.Action("PaymentCancel", "Booking"),
                Metadata = new Dictionary<string, string>
            {
                { "userId", user.Id },
                { "basketId", basket.Id.ToString() }
            }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Json(new { id = session.Id });
        }

        public async Task<IActionResult> PaymentSuccess(string session_id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(session_id))
            {
                TempData["Error"] = "Ödəniş sessiyası tapılmadı.";
                return RedirectToAction("Checkout");
            }

            var service = new SessionService();
            Stripe.Checkout.Session session;

            try
            {
                session = service.Get(session_id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödəniş məlumatlarını əldə etmək mümkün olmadı.";
                return RedirectToAction("Checkout");
            }

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Ödəniş tamamlanmayıb.";
                return RedirectToAction("Checkout");
            }

            string userId = session.Metadata.ContainsKey("userId") ? session.Metadata["userId"] : null;
            string basketIdStr = session.Metadata.ContainsKey("basketId") ? session.Metadata["basketId"] : null;

            if (userId == null || basketIdStr == null)
            {
                TempData["Error"] = "Ödəniş məlumatları tam deyil.";
                return RedirectToAction("Checkout");
            }

            if (user.Id != userId)
            {
                TempData["Error"] = "İstifadəçi məlumatı uyğun gəlmir.";
                return RedirectToAction("Checkout");
            }

            int basketId = int.Parse(basketIdStr);

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == basketId && b.UserId == userId);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbət boşdur.";
                return RedirectToAction("Checkout");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sessionGroups = basket.Items.GroupBy(i => i.SessionId);

                foreach (var group in sessionGroups)
                {
                    var booking = new Booking
                    {
                        UserId = user.Id,
                        SessionId = group.Key,
                        BookingDate = DateTime.Now,
                        TotalAmount = group.Sum(i => i.Price),
                        Status = BookingStatus.Paid
                    };
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    foreach (var item in group)
                    {
                        var ticket = await _context.Tickets
                            .FirstOrDefaultAsync(t => t.SeatId == item.SeatId &&
                                                     t.SessionId == item.SessionId &&
                                                     t.Status == TicketStatus.Reserved);

                        if (ticket != null)
                        {
                            ticket.Status = TicketStatus.Sold;
                            ticket.BookingId = booking.Id;
                            ticket.PurchasedAt = DateTime.Now;
                            _context.Tickets.Update(ticket);
                        }
                    }
                }

                _context.BasketItems.RemoveRange(basket.Items);
                basket.TotalPrice = 0;
                _context.Baskets.Update(basket);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Ödəniş uğurla tamamlandı! Biletləriniz hazırdır.";
                return RedirectToAction("MyTickets", "Account");
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Ödəniş sonrası işləmə zamanı xəta baş verdi.";
                return RedirectToAction("Checkout");
            }

        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            TempData["Warning"] = "Ödəniş ləğv edildi. Rezervasiyanız səbətdə qalır.";
            return RedirectToAction("Checkout");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromBasket(int basketItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });
            }

            var basketItem = await _context.BasketItems
                .Include(bi => bi.Basket)
                .FirstOrDefaultAsync(bi => bi.Id == basketItemId && bi.Basket.UserId == user.Id);

            if (basketItem == null)
            {
                return Json(new { success = false, message = "Element tapılmadı" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var basket = basketItem.Basket;

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.SeatId == basketItem.SeatId &&
                                            t.SessionId == basketItem.SessionId &&
                                            t.Status == TicketStatus.Reserved);

                if (ticket != null)
                {
                    _context.Tickets.Remove(ticket);
                }

                _context.BasketItems.Remove(basketItem);

                basket.TotalPrice = await _context.BasketItems
                    .Where(bi => bi.BasketId == basket.Id && bi.Id != basketItemId)
                    .SumAsync(bi => (decimal?)bi.Price) ?? 0;

                _context.Baskets.Update(basket);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Element səbətdən silindi və oturacaq sərbəst buraxıldı" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Xəta baş verdi" });
            }
        }

        private async Task<List<int>> GetTakenSeatsForSession(int sessionId)
        {
            var currentTime = DateTime.Now;

            return await _context.Tickets
                .Where(t => t.SessionId == sessionId &&
                           (t.Status == TicketStatus.Sold ||
                            (t.Status == TicketStatus.Reserved &&
                             t.PurchasedAt.AddMinutes(15) > currentTime)))
                .Select(t => t.SeatId)
                .ToListAsync();
        }

        private async Task<Basket> GetOrCreateUserBasket(string userId)
        {
            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = userId,
                    Items = new List<BasketItem>(),
                    TotalPrice = 0
                };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            return basket;
        }

        private async Task CleanExpiredReservations(string userId)
        {
            var expiredTime = DateTime.Now.AddMinutes(-15);

            var expiredTickets = await _context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.Booking.UserId == userId &&
                           t.Status == TicketStatus.Reserved &&
                           t.PurchasedAt < expiredTime)
                .ToListAsync();

            if (expiredTickets.Any())
            {
                _context.Tickets.RemoveRange(expiredTickets);

                var sessionSeatPairs = expiredTickets.Select(t => new { t.SessionId, t.SeatId }).ToList();

                foreach (var pair in sessionSeatPairs)
                {
                    var basketItem = await _context.BasketItems
                        .FirstOrDefaultAsync(bi => bi.SessionId == pair.SessionId &&
                                                 bi.SeatId == pair.SeatId &&
                                                 bi.Basket.UserId == userId);

                    if (basketItem != null)
                    {
                        _context.BasketItems.Remove(basketItem);
                    }
                }

                var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == userId);
                if (basket != null)
                {
                    basket.TotalPrice = await _context.BasketItems
                        .Where(bi => bi.BasketId == basket.Id)
                        .SumAsync(bi => (decimal?)bi.Price) ?? 0;
                    _context.Baskets.Update(basket);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}